// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Threading.Channels;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Requests.Keyboard;
using Demo.Engine.Core.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Core.Services;

internal sealed class MainLoopServiceNew
    : IMainLoopServiceNew,
      IDisposable
{
    private readonly ILogger<MainLoopServiceNew> _logger;
    private readonly ChannelWriter<StaThreadRequests> _channelWriter;
    private readonly IMediator _mediator;
    private readonly IShaderAsyncCompiler _shaderCompiler;
    private readonly IFpsTimer _fpsTimer;
    private readonly IMainLoopLifetime _mainLoopLifetime;
    private readonly ILoopJob _loopJob;
    private bool _disposedValue;

    public Task ExecutingTask { get; }

    public MainLoopServiceNew(
        ILogger<MainLoopServiceNew> logger,
        ChannelWriter<StaThreadRequests> channelWriter,
        IMediator mediator,
        IShaderAsyncCompiler shaderCompiler,
        IFpsTimer fpsTimer,
        IRenderingEngine renderingEngine,
        IMainLoopLifetime mainLoopLifetime,
        ILoopJob loopJob)
    {
        _logger = logger;
        _channelWriter = channelWriter;
        _mediator = mediator;
        _shaderCompiler = shaderCompiler;
        _fpsTimer = fpsTimer;
        _mainLoopLifetime = mainLoopLifetime;
        _loopJob = loopJob;
        ExecutingTask = DoAsync(
            renderingEngine);
    }

    private async Task DoAsync(
        IRenderingEngine renderingEngine)
    {
        _ = await _shaderCompiler.CompileShaders(_mainLoopLifetime.Token);

        var keyboardHandle = await _mediator.Send(new KeyboardHandleRequest(), CancellationToken.None);
        var keyboardCharCache = await _mediator.Send(new KeyboardCharCacheRequest(), CancellationToken.None);

        var surfaces = new RenderingSurfaceId[]
        {
            await _channelWriter.CreateSurface(
                _mainLoopLifetime.Token),
            await _channelWriter.CreateSurface(
                _mainLoopLifetime.Token),
        };

        var previous = Stopwatch.GetTimestamp();
        var lag = TimeSpan.Zero;

        var msPerUpdate = TimeSpan.FromSeconds(1) / 60;

        var doEventsFunc = StaThreadRequests.DoEventsOk(RenderingSurfaceId.Empty);
        var doEventsOk = true;

        while (
            doEventsOk
            //&& IsRunning
            && !_mainLoopLifetime.Token.IsCancellationRequested)
        {
            var current = Stopwatch.GetTimestamp();
            var elapsed = Stopwatch.GetElapsedTime(previous, current);
            previous = current;
            lag += elapsed;

            //process input
            // TODO!

            while (lag >= msPerUpdate)
            {
                //Update
                // TODO - fix the UPS timer.. somehow :D
                _fpsTimer.StopUpdateTimer();
                foreach (var renderingSurfaceId in surfaces)
                {
                    if (!renderingEngine.TryGetRenderingSurface(
                        renderingSurfaceId,
                        out var renderingSurface))
                    {
                        _logger.LogCritical(
                            "Rendering surface {id} not found!",
                            renderingSurfaceId);
                        break;
                    }

                    await _loopJob.Update(
                          renderingSurface,
                          keyboardHandle,
                          keyboardCharCache);
                }
                lag -= msPerUpdate;
                _fpsTimer.StartUpdateTimer();
            }

            //Render
            foreach (var renderingSurfaceId in surfaces)
            {
                doEventsOk &= await _channelWriter.DoEventsOk(
                    renderingSurfaceId,
                    doEventsFunc,
                    _mainLoopLifetime.Token);

                using var scope = _fpsTimer.StartRenderingTimerScope(
                    renderingSurfaceId);

                _loopJob.Render(
                    renderingEngine,
                    renderingSurfaceId);
            }
        }
        _channelWriter.Complete();
        _mainLoopLifetime.Cancel();
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
}