// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Core.Services;

internal abstract class EngineServiceBaseNew(
    ILogger<EngineServiceBaseNew> logger,
    IHostApplicationLifetime hostApplicationLifetime,
    IServiceScopeFactory scopeFactory)
    : IHostedService,
      IDisposable
{
    protected readonly ILogger<EngineServiceBaseNew> _logger = logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly string _serviceName = "Engine";
    private Task? _executingTask;
    private bool _stopRequested;
    protected IServiceProvider? _sp;
    private bool _disposedValue;

    protected bool IsRunning { get; private set; }

    private readonly string _version = Assembly
        .GetEntryAssembly()
        ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion ?? "0.0.0";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{serviceName} starting! v{version}", _serviceName, _version);
        _executingTask = DoWorkAsync();

        return _executingTask.IsCompleted
            ? _executingTask
            : Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{serviceName} stopping!", _serviceName);

        _stopRequested = true;
        if (_executingTask is null)
        {
            return;
        }

        _ = await Task.WhenAny(
            _executingTask,
            Task.Delay(Timeout.Infinite, cancellationToken));
    }

    private async Task DoWorkAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            _sp = scope.ServiceProvider;
            var osMessageHandler = _sp.GetRequiredService<IOSMessageHandler>();
            var renderingEngine = _sp.GetRequiredService<IRenderingEngine>();

            IsRunning = true;

            var executeAsync = RunDoAsync(
                renderingEngine);

            var runStaThread = RunSTAThread(
                renderingEngine,
                osMessageHandler);

            await Task.WhenAll(
                [
                    executeAsync,
                    runStaThread
                ]);

            _sp = null;
        }
        catch (OperationCanceledException)
        {
            IsRunning = false;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "{serviceName} failed with error! {errorMessage}", _serviceName, ex.Message);
        }
        finally
        {
            if (!_stopRequested)
            {
                _hostApplicationLifetime.StopApplication();
            }
        }
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
                SingleThreadedSynchronizationContext.Await(async ()
                    => await STAThread(
                        renderingEngine: renderingEngine,
                        osMessageHandler: osMessageHandler,
                        cancellationToken: _hostApplicationLifetime.ApplicationStopping));

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

    private async Task RunDoAsync(
        IRenderingEngine renderingEngine)
    {
        await DoAsync(
            renderingEngine: renderingEngine,
            cancellationToken: _hostApplicationLifetime.ApplicationStopping);
        IsRunning = false;
    }

    protected abstract Task STAThread(
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler,
        CancellationToken cancellationToken);

    protected abstract Task DoAsync(
        IRenderingEngine renderingEngine,
        CancellationToken cancellationToken);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _hostApplicationLifetime.StopApplication();
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

    internal sealed class SingleThreadedSynchronizationContext
            : SynchronizationContext
    {
        private readonly BlockingCollection<(SendOrPostCallback d, object? state)> _queue = [];

        public override void Post(SendOrPostCallback d, object? state) => _queue.Add((d, state));

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