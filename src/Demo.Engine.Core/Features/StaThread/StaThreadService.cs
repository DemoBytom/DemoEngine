// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Core.Features.StaThread;

internal sealed class StaThreadService
    : IStaThreadService
{
    private readonly ILogger<StaThreadService> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ChannelReader<StaThreadRequests> _channelReader;
    private readonly IMainLoopLifetime _mainLoopLifetime;

    public Task ExecutingTask { get; }
    public bool IsRunning { get; private set; }

    public StaThreadService(
        ILogger<StaThreadService> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler,
        ChannelReader<StaThreadRequests> channelReader,
        IMainLoopLifetime mainLoopLifetime)
    {
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _channelReader = channelReader;
        _mainLoopLifetime = mainLoopLifetime;
        IsRunning = true;
        ExecutingTask = RunSTAThread(
            renderingEngine,
            osMessageHandler);
    }

    private async Task RunSTAThread(
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler)
    {
        var originalSynContext = SynchronizationContext.Current;
        StaSingleThreadedSynchronizationContext? syncContext = null;
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            _hostApplicationLifetime.ApplicationStopping,
            _mainLoopLifetime.Token);

        try
        {

            syncContext = new StaSingleThreadedSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(syncContext);
            // Task.Yield is used here to force a context switch to STA thread
            // Awaiting here allows the STA thread to start and pump messages, which is necessary for the STAThread method to function correctly.
            await Task.Yield();
            System.Diagnostics.Debug.Assert(
                SynchronizationContext.Current == syncContext,
                "SynchronizationContext should be set to the STA context.");

            try
            {
                syncContext.OnError += OnStaContextError;

                await STAThread(
                        renderingEngine,
                        osMessageHandler,
                        cts.Token)
                    .ConfigureAwait(continueOnCapturedContext: true);
            }
            catch (OperationCanceledException)
            {
                _logger.LogStaThreadServiceWasCancelled();
            }
            catch (Exception)
            {
                IsRunning = false;
                _mainLoopLifetime.Cancel();
                throw;
            }
            IsRunning = false;
            _mainLoopLifetime.Cancel();

        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(originalSynContext);
            // Force context switch back to original context to ensure all STA thread work is completed before disposing the syncContext
            await Task.Yield();
            syncContext?.OnError -= OnStaContextError;
            syncContext?.Dispose();
        }

        void OnStaContextError(object? sender, Exception ex)
        {
            _logger.LogCritical(ex,
                "An error occured within the STA thread synchronization context.");
            cts.Cancel();
        }
    }

    private async Task STAThread(
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler,
        CancellationToken cancellationToken)
    {
        var doEventsOk = true;

        await foreach (var staAction in _channelReader
            .ReadAllAsync(cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: true)
            .WithCancellation(cancellationToken))
        {
            switch (staAction)
            {
                case StaThreadRequests.DoEventsOkRequest doEventsOkRequest:
                    doEventsOk &= await doEventsOkRequest
                        .InvokeAsync(renderingEngine, osMessageHandler, cancellationToken)
                        .ConfigureAwait(continueOnCapturedContext: true);
                    break;

                default:
                    _ = await staAction
                        .InvokeAsync(renderingEngine, osMessageHandler, cancellationToken)
                        .ConfigureAwait(continueOnCapturedContext: true);
                    break;
            }

            if (!doEventsOk || !IsRunning || cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    internal sealed class StaSingleThreadedSynchronizationContext
        : SynchronizationContext,
          IDisposable
    {
        private readonly BlockingCollection<WorkItem> _workQueue = [];

        private readonly Thread _thread;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _isDisposed;

        public event EventHandler<Exception>? OnError;

        /// <inheritdoc/>
        public override void Post(SendOrPostCallback d, object? state)
            => _workQueue.Add(WorkItem.Asynchronous(d, state));

        /// <inheritdoc/>
        public override void Send(SendOrPostCallback d, object? state)
        {
            // If we're already on the STA thread, execute the callback directly to avoid deadlock.
            if (Environment.CurrentManagedThreadId == _thread.ManagedThreadId)
            {
                d.Invoke(state);
                return;
            }

            var workItem = WorkItem.Synchronous(d, state);

            try
            {
                _workQueue.Add(workItem);
                workItem.SyncEvent!.Wait();

                if (workItem.Exception is { } exception)
                {
                    throw exception;
                }
            }
            finally
            {
                workItem.Dispose();
            }
        }

        public StaSingleThreadedSynchronizationContext()
        {
            _thread = new Thread(ThreadInner)
            {
                IsBackground = false,
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //Can only by set on the Windows machine. Doesn't work on Linux/MacOS
                _thread.SetApartmentState(ApartmentState.STA);
                _thread.Name = "Main STA thread";
            }
            else
            {
                _thread.Name = "Main thread";
            }

            _thread.Start();
        }

        private void ThreadInner()
        {
            SetSynchronizationContext(this);
            try
            {
                foreach (var workItem in _workQueue.GetConsumingEnumerable(_cancellationTokenSource.Token))
                {
                    try
                    {
                        workItem.Callback.Invoke(workItem.State);
                    }
                    catch (Exception ex)
                    {
                        if (workItem.IsSynchronous)
                        {
                            workItem.Exception = ex;
                        }
                        else
                        {
                            /* An exception cannot be thrown to the caller from here
                             * since the caller has already continued execution after posting the work item. 
                             * Instead an event is rised, if there's subscribers, or the process is terminated
                             * If there are subscribers, but they don't cancel the sync context within 10 seconds
                             * the process is terminated to avoid running in an unstable state. */
                            if (OnError is not null)
                            {
                                OnError.Invoke(this, ex);
                                _ = ExitAfter10Seconds(_cancellationTokenSource.Token);
                            }
                            else
                            {
                                Environment.Exit(-1);
                            }
                        }

                        static async Task ExitAfter10Seconds(CancellationToken cancellationToken)
                        {
                            try
                            {
                                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken)
                                    .ConfigureAwait(false);
                            }
                            catch (Exception)
                                when (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }

                            Environment.Exit(-1);
                        }
                    }
                    finally
                    {
                        if (workItem.IsSynchronous)
                        {
                            workItem.SyncEvent.Set();
                        }
                        else
                        {
                            workItem.Dispose();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Gracefully exit the thread when cancellation is requested.
            }
            catch (InvalidOperationException)
            {
                // The BlockingCollection has been marked as complete for adding, which means we're shutting down.
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;

            // Gracefully shutdown the message pump
            _cancellationTokenSource.Cancel();
            _workQueue.CompleteAdding();

            _thread.Join();
            _workQueue.Dispose();
            _cancellationTokenSource.Dispose();
        }

        private sealed class WorkItem
            : IDisposable
        {
            private bool _isDisposed;

            public SendOrPostCallback Callback { get; }

            public object? State { get; }

            [MemberNotNullWhen(true, nameof(SyncEvent))]
            public bool IsSynchronous { get; }

            public ManualResetEventSlim? SyncEvent { get; }

            public Exception? Exception { get; set; }

            private WorkItem(
                SendOrPostCallback callback,
                object? state,
                bool isSynchronous)
            {
                Callback = callback;
                State = state;
                IsSynchronous = isSynchronous;
                if (isSynchronous)
                {
                    SyncEvent = new ManualResetEventSlim();
                }
            }

            public static WorkItem Synchronous(SendOrPostCallback callback, object? state)
                => new(
                    callback: callback,
                    state: state,
                    isSynchronous: true);

            public static WorkItem Asynchronous(SendOrPostCallback callback, object? state)
                => new(
                    callback: callback,
                    state: state,
                    isSynchronous: false);

            public void Dispose()
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;

                SyncEvent?.Dispose();
            }
        }
    }
}