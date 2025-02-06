// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Numerics;
using System.Threading.Channels;
using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Platform;
using Demo.Engine.Core.Requests.Keyboard;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vortice.Mathematics;

namespace Demo.Engine.Core.Services;

internal class EngineServiceNew(
    ILogger<EngineServiceNew> logger,
    IHostApplicationLifetime hostApplicationLifetime,
    IServiceScopeFactory scopeFactory,
    IMediator mediator,
    IShaderAsyncCompiler shaderCompiler,
    IFpsTimer fpsTimer)
    : EngineServiceBaseNew(
        logger,
        hostApplicationLifetime,
        scopeFactory)
{
    private readonly IMediator _mediator = mediator;
    private readonly IShaderAsyncCompiler _shaderCompiler = shaderCompiler;
    private readonly IFpsTimer _fpsTimer = fpsTimer;
    private ICube[] _drawables = [];
    private readonly CancellationTokenSource _loopCancellationTokenSource = new();
    private bool _disposedValue;

    internal record StaThreadWork(
        Action<IRenderingEngine> Action);

    private readonly Channel<StaThreadWork> _channel = Channel.CreateBounded<StaThreadWork>(
        new BoundedChannelOptions(10)
        {
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        });

    protected override async Task DoAsync(
        IRenderingEngine renderingEngine,
        CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _loopCancellationTokenSource.Token);

        _ = await _shaderCompiler.CompileShaders(cts.Token);

        var keyboardHandle = await _mediator.Send(new KeyboardHandleRequest(), CancellationToken.None);
        var keyboardCharCache = await _mediator.Send(new KeyboardCharCacheRequest(), CancellationToken.None);

        _drawables =
        [
            _sp!.GetRequiredService<ICube>(),
        ];

        await _channel.Writer.WriteAsync(
            new StaThreadWork(
                re => re.CreateSurface()),
            cts.Token);

        await _channel.Writer.WriteAsync(
            new StaThreadWork(
                re => re.CreateSurface()),
            cts.Token);

        var previous = Stopwatch.GetTimestamp();
        var lag = TimeSpan.Zero;

        var msPerUpdate = TimeSpan.FromSeconds(1) / 60;

        while (
            IsRunning
            && !cts.Token.IsCancellationRequested)
        {
            var current = Stopwatch.GetTimestamp();
            var elapsed = Stopwatch.GetElapsedTime(previous, current);
            previous = current;
            lag += elapsed;
            //process input

            while (lag >= msPerUpdate)
            {
                //Update
                foreach (var renderingSurface in renderingEngine.RenderingSurfaces)
                {
                    await Update(
                          renderingSurface,
                          keyboardHandle,
                          keyboardCharCache);

                    lag -= msPerUpdate;
                }
            }

            //Render
            foreach (var renderingSurface in renderingEngine.RenderingSurfaces)
            {
                _fpsTimer.Start();
                await Render(
                    renderingEngine,
                    renderingSurface.ID);
                _fpsTimer.Stop();
            }
        }
    }

    protected override async Task STAThread(
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler,
        CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _loopCancellationTokenSource.Token);

        var doEventsOk = true;

        using var periodicTimer = new PeriodicTimer(
            TimeSpan.FromSeconds(1) / 60);

        var timestamp = Stopwatch.GetTimestamp();

        while (await periodicTimer.WaitForNextTickAsync(cts.Token)
                && IsRunning
                && doEventsOk
                && !cts.Token.IsCancellationRequested)
        {
            //_logger.LogTrace("Tick! {elapsedMS}",
            //    Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds);
            timestamp = Stopwatch.GetTimestamp();

            while (_channel.Reader.TryRead(out var staAction))
            {
                staAction.Action.Invoke(renderingEngine);
            }
            foreach (var renderingSurface in renderingEngine.RenderingSurfaces)
            {
                doEventsOk &= osMessageHandler.DoEvents(
                        renderingSurface.RenderingControl);
            }
        }
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
            _loopCancellationTokenSource.Cancel();
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
            && _sp is not null)
        {
            Debug.WriteLine("Adding new Cube!");
            _drawables = new List<ICube>(_drawables)
                {
                    _sp.GetRequiredService<ICube>()
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
        _r = MathF.Sin((_sin + 0) * MathF.PI / 180);
        _g = MathF.Sin((_sin + 120) * MathF.PI / 180);
        _b = MathF.Sin((_sin + 240) * MathF.PI / 180);

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

    private ValueTask Render(
        IRenderingEngine renderingEngine,
        Guid renderingSurfaceId)
    {
        renderingEngine.Draw(
            color: new Color4(_r, _g, _b, 1.0f),
            renderingSurfaceId: renderingSurfaceId,
            drawables: _drawables);

        //if (renderingEngine.Control.IsFullscreen != _fullscreen)
        //{
        //    renderingEngine.SetFullscreen(_fullscreen);
        //}

        return ValueTask.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _loopCancellationTokenSource.Dispose();

                foreach (var drawable in _drawables)
                {
                    if (drawable is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                _disposedValue = true;
            }
            base.Dispose(disposing);
        }
    }
}