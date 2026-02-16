// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

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
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                _hostApplicationLifetime.ApplicationStopping,
                _mainLoopLifetime.Token);

            try
            {

                SingleThreadedSynchronizationContextChannel.Await(async ()
                    => await STAThread(
                        renderingEngine: renderingEngine,
                        osMessageHandler: osMessageHandler,
                        cancellationToken: cts.Token));

                tcs.SetResult();
            }
            catch (OperationCanceledException)
            {
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            IsRunning = false;
            _mainLoopLifetime.Cancel();
        });
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //Can only by set on the Windows machine. Doesn't work on Linux/MacOS
            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "Main STA thread";
        }
        else
        {
            thread.Name = "Main thread";
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