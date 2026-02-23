// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Microsoft.Extensions.Hosting;

namespace Demo.Engine.Core.Features.StaThread;

internal sealed class StaThreadService
    : IStaThreadService,
      IDisposable
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ChannelReader<StaThreadRequests> _channelReader;
    private readonly IMainLoopLifetime _mainLoopLifetime;
    private bool _disposedValue;
    private Thread? _thread;

    public Task ExecutingTask { get; }
    public bool IsRunning { get; private set; }

    public StaThreadService(
        IHostApplicationLifetime hostApplicationLifetime,
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler,
        ChannelReader<StaThreadRequests> channelReader,
        IMainLoopLifetime mainLoopLifetime)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _channelReader = channelReader;
        _mainLoopLifetime = mainLoopLifetime;
        IsRunning = true;
        ExecutingTask = RunSTAThread2(
            renderingEngine,
            osMessageHandler);
    }

    private Task RunSTAThread(
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler)
    {
        var tcs = new TaskCompletionSource();
        _thread = new Thread(()
            =>
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                _hostApplicationLifetime.ApplicationStopping,
                _mainLoopLifetime.Token);

            try
            {

                SingleThreadedSynchronizationContextChannel.Await(()
                    => STAThread(
                        renderingEngine: renderingEngine,
                        osMessageHandler: osMessageHandler,
                        cancellationToken: cts.Token));

                FinishRunning(tcs);
            }
            catch (OperationCanceledException)
            {
                FinishRunning(tcs);
            }
            catch (Exception ex)
            {
                FinishRunning(tcs, ex);
            }
        });
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

        return tcs.Task;

        void FinishRunning(
            TaskCompletionSource tcs,
            Exception? exception = null)
        {
            /* This should be called BEFORE tcs.SetResult/tcs.SetException!
             * Otherwise _mainLoopLifetime.Cancel() gets called after the returned tcs.Task completes,
             * leading to dispoes exception on mainLoopLifetime, that's already disposed upstream! */
            IsRunning = false;
            _mainLoopLifetime.Cancel();

            if (exception is null)
            {
                tcs.SetResult();
            }
            else
            {
                tcs.SetException(exception);
            }
        }
    }

    private async Task RunSTAThread2(
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler)
    {
        var originalSynContext = SynchronizationContext.Current;
        StaSingleThreadedSynchronizationContext? syncContext = null;
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                _hostApplicationLifetime.ApplicationStopping,
                _mainLoopLifetime.Token);

            syncContext = new StaSingleThreadedSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(syncContext);
            // Task.Yield is used here to force a context switch to STA thread
            // Awaiting here allows the STA thread to start and pump messages, which is necessary for the STAThread method to function correctly.
            await Task.Yield();
            try
            {
                await STAThread(
                    renderingEngine,
                    osMessageHandler,
                    cts.Token);

                IsRunning = false;
                _mainLoopLifetime.Cancel();
            }
            catch (OperationCanceledException)
            {
                IsRunning = false;
                _mainLoopLifetime.Cancel();
            }
            catch (Exception)
            {
                IsRunning = false;
                _mainLoopLifetime.Cancel();
                throw;
            }
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(originalSynContext);
            // Force context switch back to original context to ensure all STA thread work is completed before disposing the syncContext
            await Task.Yield();
            syncContext?.Dispose();
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
            .WithCancellation(cancellationToken))
        {
            switch (staAction)
            {
                case StaThreadRequests.DoEventsOkRequest doEventsOkRequest:
                    doEventsOk &= doEventsOkRequest.Invoke(renderingEngine, osMessageHandler);
                    break;

                default:
                    _ = staAction.Invoke(renderingEngine, osMessageHandler);
                    break;
            }

            if (!doEventsOk || !IsRunning || cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _thread?.Join();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private sealed class StaSingleThreadedSynchronizationContext
        : SynchronizationContext,
          IDisposable
    {
        private readonly BlockingCollection<WorkItem> _workQueue = [];

        private readonly Thread _thread;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _isDisposed;

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
                            // Handle/log exception for asynchronous work items as needed.
                            // An exception cannot be thrown to the caller from here since the caller has already continued execution after posting the work item.
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

    private sealed class SingleThreadedSynchronizationContextChannel
        : SynchronizationContext
    {
        private readonly Channel<(SendOrPostCallback d, object? state)> _channel =
            Channel.CreateUnbounded<(SendOrPostCallback d, object? state)>(
                new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

        public override void Post(SendOrPostCallback d, object? state)
            => _channel.Writer.TryWrite((d, state));

        public override void Send(SendOrPostCallback d, object? state)
            => throw new InvalidOperationException("Synchronous operations are not supported!");

        public static void Await(Func<Task> taskInvoker)
        {
            var originalContext = Current;
            try
            {
                var context = new SingleThreadedSynchronizationContextChannel();
                SetSynchronizationContext(context);

                Task task;
                try
                {
                    task = taskInvoker.Invoke();
                }
                catch (Exception ex)
                {
                    // If the invoker throws synchronously, complete the channel so the pump can exit.
                    context._channel.Writer.Complete(ex);
                    throw;
                }

                _ = task.ContinueWith(t
                    => context._channel.Writer.Complete(t.Exception),
                    TaskScheduler.Default);

                // Pump loop: block synchronously until items are available or the writer completes.
                while (context._channel.Reader.WaitToReadAsync().Preserve().GetAwaiter().GetResult())
                {
                    while (context._channel.Reader.TryRead(out var work))
                    {
                        work.d.Invoke(work.state);
                    }
                }

                task.GetAwaiter().GetResult();
            }
            finally
            {
                SetSynchronizationContext(originalContext);
            }
        }
    }
}