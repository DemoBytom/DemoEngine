// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Microsoft.Extensions.Hosting;

namespace Demo.Engine.Core.Services;

internal sealed class StaThreadService
    : IStaThreadService,
      IDisposable
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ChannelReader<StaThreadRequests> _channelReader;
    private bool _disposedValue;
    private readonly CancellationTokenSource _loopCancellationTokenSource = new();

    public Task ExecutingTask { get; }
    public bool IsRunning { get; private set; }

    public StaThreadService(
        IHostApplicationLifetime hostApplicationLifetime,
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler,
        ChannelReader<StaThreadRequests> channelReader)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _channelReader = channelReader;
        IsRunning = true;
        ExecutingTask = RunSTAThread(
            renderingEngine,
            osMessageHandler);
    }

    private Task RunSTAThread(
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler)
    {
        var tcs = new TaskCompletionSource();
        var thread = new Thread(()
            =>
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                    _hostApplicationLifetime.ApplicationStopping,
                    _loopCancellationTokenSource.Token);

                SingleThreadedSynchronizationContext.Await(async ()
                    => await STAThread(
                        renderingEngine: renderingEngine,
                        osMessageHandler: osMessageHandler,
                        cancellationToken: cts.Token));

                IsRunning = false;
                tcs.SetResult();
            }
            catch (OperationCanceledException)
            {
                IsRunning = false;
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //Can only by set on the Windows machine. Doesn't work on Linux/MacOS
            thread.SetApartmentState(ApartmentState.STA);
        }

        thread.Start();

        return tcs.Task;
    }

    private async Task STAThread(
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler,
        CancellationToken cancellationToken)
    {
        var doEventsOk = true;
        while (
            await _channelReader.WaitToReadAsync(cancellationToken)
                && IsRunning
                && doEventsOk
                && !cancellationToken.IsCancellationRequested)
        {
            while (
            doEventsOk
                && _channelReader.TryRead(out var staAction))
            {
                if (staAction is StaThreadRequests.DoEventsOkRequest doEventsOkRequest)
                {
                    doEventsOk &= doEventsOkRequest.Invoke(renderingEngine, osMessageHandler);
                }
                else
                {
                    _ = staAction.Invoke(renderingEngine, osMessageHandler);
                }
            }
        }

        _loopCancellationTokenSource.Cancel();
    }

    public void Cancel()
        => _loopCancellationTokenSource.Cancel();

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _loopCancellationTokenSource.Dispose();
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

    private sealed class SingleThreadedSynchronizationContext
            : SynchronizationContext
    {
        private readonly BlockingCollection<(SendOrPostCallback d, object? state)> _queue = [];

        public override void Post(SendOrPostCallback d, object? state)
            => _queue.Add((d, state));

        public override void Send(SendOrPostCallback d, object? state)
            => throw new InvalidOperationException(
                "Synchronous operations are not supported!");

        public static void Await(
            Func<Task> taskInvoker)
        {
            var originalContext = Current;
            try
            {
                var context = new SingleThreadedSynchronizationContext();
                SetSynchronizationContext(context);

                var task = taskInvoker.Invoke();
                _ = task.ContinueWith(_ => context._queue.CompleteAdding());

                while (context._queue.TryTake(out var work, Timeout.Infinite))
                {
                    work.d.Invoke(work.state);
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