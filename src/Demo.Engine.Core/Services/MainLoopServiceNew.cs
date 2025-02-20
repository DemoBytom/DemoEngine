// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Numerics;
using System.Threading.Channels;
using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Platform;
using Demo.Engine.Core.Requests.Keyboard;
using Demo.Engine.Core.ValueObjects;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vortice.Mathematics;

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
    private readonly IServiceProvider _serviceProvider;
    private readonly IMainLoopLifetime _mainLoopLifetime;
    private ICube[] _drawables = [];
    private bool _disposedValue;

    public Task ExecutingTask { get; }

    public MainLoopServiceNew(
        ILogger<MainLoopServiceNew> logger,
        ChannelWriter<StaThreadRequests> channelWriter,
        IMediator mediator,
        IShaderAsyncCompiler shaderCompiler,
        IFpsTimer fpsTimer,
        IServiceProvider serviceProvider,
        IRenderingEngine renderingEngine,
        IMainLoopLifetime mainLoopLifetime)
    {
        _logger = logger;
        _channelWriter = channelWriter;
        _mediator = mediator;
        _shaderCompiler = shaderCompiler;
        _fpsTimer = fpsTimer;
        _serviceProvider = serviceProvider;
        _mainLoopLifetime = mainLoopLifetime;
        ExecutingTask = DoAsync(
            renderingEngine);
    }

    private async Task DoAsync(
        IRenderingEngine renderingEngine)
    {
        _ = await _shaderCompiler.CompileShaders(_mainLoopLifetime.Token);

        var keyboardHandle = await _mediator.Send(new KeyboardHandleRequest(), CancellationToken.None);
        var keyboardCharCache = await _mediator.Send(new KeyboardCharCacheRequest(), CancellationToken.None);

        _drawables =
        [
            _serviceProvider.GetRequiredService<ICube>(),
        ];

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

                    await Update(
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

                Render(
                    renderingEngine,
                    renderingSurfaceId);
            }
        }
        _channelWriter.Complete();
        _mainLoopLifetime.Cancel();
    }

    private float _r, _g, _b = 0.0f;

    private float _sin = 0.0f;
    private bool _fullscreen = false;
    private bool _f11Pressed = false;

    private ValueTask Update(
        IRenderingSurface renderingSurface,
        KeyboardHandle keyboardHandle,
        KeyboardCharCache keyboardCharCache)
    {
        if (keyboardHandle.GetKeyPressed(VirtualKeys.OemOpenBrackets))
        {
            keyboardCharCache.Clear();
        }
        if (keyboardHandle.GetKeyPressed(VirtualKeys.OemCloseBrackets))
        {
            var str = keyboardCharCache?.ReadCache();
            if (!string.IsNullOrEmpty(str))
            {
                _logger.LogInformation(str);
            }
        }
        if (keyboardHandle.GetKeyPressed(VirtualKeys.Escape))
        {
            _mainLoopLifetime.Cancel();
            foreach (var drawable in _drawables)
            {
                (drawable as IDisposable)?.Dispose();
            }

            _drawables = [];
            return ValueTask.CompletedTask;
        }
        if (keyboardHandle.GetKeyPressed(VirtualKeys.F11))
        {
            if (!_f11Pressed)
            {
                _fullscreen = !_fullscreen;
            }
            _f11Pressed = true;
        }
        else
        {
            _f11Pressed = false;
        }

        if (keyboardHandle.GetKeyPressed(VirtualKeys.Back)
            && _drawables.ElementAtOrDefault(0) is IDisposable d)
        {
            Debug.WriteLine("Removing cube!");

            _drawables = _drawables.Length > 0
                ? _drawables[1..]
                : [];

            d?.Dispose();

            Debug.WriteLine("Cube removed!");
        }
        if (keyboardHandle.GetKeyPressed(VirtualKeys.Enter)
            && _drawables.Length < 2
            && _serviceProvider is not null)
        {
            Debug.WriteLine("Adding new Cube!");
            _drawables = new List<ICube>(_drawables)
                {
                    _serviceProvider.GetRequiredService<ICube>()
                }.ToArray();
            Debug.WriteLine("Cube added!!");
        }

        //if (_drawables.Length == 2)
        //{
        //    foreach (var drawable in _drawables)
        //    {
        //        (drawable as IDisposable)?.Dispose();
        //    }

        //    _drawables = Array.Empty<ICube>();
        //}
        //else if (_drawables.Length < 2 && _sp is not null /*&& _dontCreate == false*/)
        //{
        //    _drawables = new List<ICube>(_drawables)
        //        {
        //            _sp.GetRequiredService<ICube>()
        //        }.ToArray();
        //    //_dontCreate = true;
        //}

        //Share the rainbow
        _r = float.Sin((_sin + 0) * float.Pi / 180);
        _g = float.Sin((_sin + 120) * float.Pi / 180);
        _b = float.Sin((_sin + 240) * float.Pi / 180);

        //Taste the rainbow
        if (++_sin > 360)
        {
            _sin = 0;
        }
        _angleInRadians = (_angleInRadians + 0.01f) % TWO_PI;

        _drawables.ElementAtOrDefault(0)
            ?.Update(renderingSurface, Vector3.Zero, _angleInRadians);
        _drawables.ElementAtOrDefault(1)
            ?.Update(renderingSurface, new Vector3(0.5f, 0.0f, -0.5f), -_angleInRadians * 1.5f);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// https://bitbucket.org/snippets/DemoBytom/aejA59/maps-value-between-from-one-min-max-range
    /// </summary>
    public static float Map(float value, float inMin, float inMax, float outMin, float outMax)
        => ((value - inMin) * (outMax - outMin) / (inMax - inMin)) + outMin;

    private float _angleInRadians = 0.0f;
    private const float TWO_PI = MathHelper.TwoPi;

    private void Render(
        IRenderingEngine renderingEngine,
        RenderingSurfaceId renderingSurfaceId)
        => renderingEngine.Draw(
            color: new Color4(_r, _g, _b, 1.0f),
            renderingSurfaceId: renderingSurfaceId,
            drawables: _drawables);

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                foreach (var drawable in _drawables)
                {
                    if (drawable is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
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