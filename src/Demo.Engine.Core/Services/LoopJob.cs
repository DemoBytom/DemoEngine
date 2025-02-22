// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Numerics;
using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Platform;
using Demo.Engine.Core.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vortice.Mathematics;

namespace Demo.Engine.Core.Services;

internal sealed class LoopJob
    : ILoopJob,
      IDisposable
{
    private readonly ILogger<LoopJob> _logger;
    private readonly IMainLoopLifetime _mainLoopLifetime;
    private readonly IServiceProvider _serviceProvider;

    private ICube[] _drawables = [];
    private bool _disposedValue;

    public LoopJob(
        ILogger<LoopJob> logger,
        IMainLoopLifetime mainLoopLifetime,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _mainLoopLifetime = mainLoopLifetime;
        _serviceProvider = serviceProvider;

        _drawables =
        [
            _serviceProvider.GetRequiredService<ICube>(),
        ];
    }

    private float _r, _g, _b = 0.0f;

    private float _sin = 0.0f;
    private bool _fullscreen = false;
    private bool _f11Pressed = false;

    public ValueTask Update(
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

    public void Render(
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