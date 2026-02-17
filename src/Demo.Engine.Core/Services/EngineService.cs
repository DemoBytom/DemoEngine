// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Reflection;
using Demo.Engine.Core.Features.StaThread;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Core.Services;

internal sealed class EngineService(
    ILogger<EngineService> logger,
    IHostApplicationLifetime hostApplicationLifetime,
    IServiceScopeFactory scopeFactory)
    : IHostedService,
      IDisposable
{
    private readonly ILogger<EngineService> _logger = logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly string _serviceName = "Engine";
    private Task? _executingTask;
    private bool _stopRequested;
    private bool _disposedValue;

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
        using var scope = _scopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var mainLoopLifetime = serviceProvider.GetService<IMainLoopLifetime>();

        try
        {
            var osMessageHandler = serviceProvider.GetRequiredService<IOSMessageHandler>();
            var renderingEngine = serviceProvider.GetRequiredService<IRenderingEngine>();

            var mainLoopService = serviceProvider.GetRequiredService<IMainLoopService>();
            var executeAsync = mainLoopService.ExecutingTask;

            var staThreadService = serviceProvider.GetRequiredService<IStaThreadService>();
            var runStaThread = staThreadService.ExecutingTask;

            await Task.WhenAll(
                [
                    executeAsync,
                    runStaThread
                ]);
        }
        catch (OperationCanceledException)
        {
            mainLoopLifetime?.Cancel();
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

    private void Dispose(bool disposing)
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
}