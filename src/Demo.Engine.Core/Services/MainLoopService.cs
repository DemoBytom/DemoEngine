// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using Demo.Engine.Core.Features.StaThread;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Requests.Keyboard;
using Demo.Engine.Core.ValueObjects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Core.Services;

internal sealed class MainLoopService
    : IMainLoopService,
      IAsyncDisposable
{
    private readonly ILogger<MainLoopService> _logger;
    private readonly IStaThreadWriter _staThreadWriter;
    private readonly IMediator _mediator;
    private readonly IFpsTimer _fpsTimer;
    private readonly IMainLoopLifetime _mainLoopLifetime;
    private readonly ILoopJob _loopJob;
    private bool _disposedValue;

    public Task ExecutingTask { get; }

    public MainLoopService(
        ILogger<MainLoopService> logger,
        IStaThreadWriter staThreadWriter,
        IMediator mediator,
        IFpsTimer fpsTimer,
        IRenderingEngine renderingEngine,
        IMainLoopLifetime mainLoopLifetime,
        ILoopJob loopJob)
    {
        _logger = logger;
        _staThreadWriter = staThreadWriter;
        _mediator = mediator;
        _fpsTimer = fpsTimer;
        _mainLoopLifetime = mainLoopLifetime;
        _loopJob = loopJob;
        ExecutingTask = Task.Run(async () =>
        {
            try
            {
                await DoAsync(renderingEngine);
            }
            catch (TaskCanceledException)
            {
                _mainLoopLifetime.Cancel();
            }
            catch (Exception ex)
            {
                _logger.LogMainLoopFailedWithError(ex);
                _mainLoopLifetime.Cancel();
                //throw;
            }
        });
    }

    private async Task DoAsync(
        IRenderingEngine renderingEngine)
    {
        var keyboardHandle = await _mediator.Send(new KeyboardHandleRequest(), CancellationToken.None);
        var keyboardCharCache = await _mediator.Send(new KeyboardCharCacheRequest(), CancellationToken.None);

        var surfaces = new List<RenderingSurfaceId>
        {
            await _staThreadWriter.CreateSurface(
                renderingEngine,
                _mainLoopLifetime.Token),
            //await _staThreadWriter.CreateSurface(
            //    _mainLoopLifetime.Token),
        };

        if (surfaces is [var mainFormId, ..]
            && renderingEngine.TryGetRenderingSurface(mainFormId, out var mainForm))
        {
            mainForm.RenderingControl.RenderingFormClosed += (_, _) => _mainLoopLifetime.Cancel();
        }

        var previous = Stopwatch.GetTimestamp();
        var lag = TimeSpan.Zero;

        var msPerUpdate = TimeSpan.FromSeconds(1) / 60;

        var cnt = 0;
        while (
            //&& IsRunning
            !_disposedValue
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
                RenderingSurfaceId? sId = null;

                ++cnt;
                foreach (var renderingSurfaceId in surfaces)
                {
                    if (!renderingEngine.TryGetRenderingSurface(
                        renderingSurfaceId,
                        out var renderingSurface))
                    {
                        _logger.LogRenderingSurfaceNotFound(
                            renderingSurfaceId);
                        break;
                    }

                    await _loopJob.Update(
                          renderingSurface,
                          keyboardHandle,
                          keyboardCharCache);

                    if (cnt == 10 * 60)
                    {
                        sId = await _staThreadWriter.CreateSurface(
                            renderingEngine,
                            _mainLoopLifetime.Token);

                        cnt = 0;
                    }
                }
                if (sId is not null)
                {
                    surfaces.Add(sId.Value);
                    sId = null;
                }
                lag -= msPerUpdate;
                _fpsTimer.StartUpdateTimer();
            }

            //Render
            foreach (var renderingSurfaceId in surfaces)
            {
                using var scope = _fpsTimer.StartRenderingTimerScope(
                    renderingSurfaceId);

                _loopJob.Render(
                    renderingEngine,
                    renderingSurfaceId);
            }
        }
        _mainLoopLifetime.Cancel();
    }

    private async ValueTask Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
            }

            _disposedValue = true;
            //Make sure the loop finishes
            _mainLoopLifetime.Cancel();
            await ExecutingTask;
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        await Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}