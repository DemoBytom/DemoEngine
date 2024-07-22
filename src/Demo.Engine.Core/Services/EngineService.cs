// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;
using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vortice.Mathematics;

namespace Demo.Engine.Core.Services;

public class EngineService : ServiceBase
{
    private bool _disposedValue;
    private readonly CancellationTokenSource _loopCancellationTokenSource = new();
    private readonly IServiceScopeFactory _scopeFactory;

    public EngineService(
        IHostApplicationLifetime applicationLifetime,
        ILogger<EngineService> logger,
        IServiceScopeFactory scopeFactory)
        : base("Engine",
              logger, applicationLifetime)
        => _scopeFactory = scopeFactory;

    private IServiceProvider? _sp;

    protected override async Task ExecuteAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        _sp = scope.ServiceProvider;
        _drawables = Array.Empty<ICube>(); /* new[]
            {
                    scope.ServiceProvider.GetRequiredService<ICube>(),
                    //scope.ServiceProvider.GetRequiredService<ICube>()
            };*/
        var mainLoop = scope.ServiceProvider.GetRequiredService<IMainLoopService>();

        await mainLoop.RunAsync(
            Update,
            Render,
            _loopCancellationTokenSource.Token);
        _sp = null;
    }

    private float _r, _g, _b = 0.0f;

    private float _sin = 0.0f;
    private bool _fullscreen = false;
    private bool _f11Pressed = false;

    //private bool _dontCreate = false;
    private Task Update(
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
            System.Diagnostics.Debug.WriteLine("Removing cube!");

            _drawables = _drawables.Length > 0
                ? _drawables[1..]
                : Array.Empty<ICube>();

            d?.Dispose();

            System.Diagnostics.Debug.WriteLine("Cube removed!");
        }
        if (keyboardHandle.GetKeyPressed(VirtualKeys.Enter)
            && _drawables.Length < 2
            && _sp is not null)
        {
            System.Diagnostics.Debug.WriteLine("Adding new Cube!");
            _drawables = new List<ICube>(_drawables)
                {
                    _sp.GetRequiredService<ICube>()
                }.ToArray();
            System.Diagnostics.Debug.WriteLine("Cube added!!");
        }

        if (_drawables.Length == 2)
        {
            foreach (var drawable in _drawables)
            {
                (drawable as IDisposable)?.Dispose();
            }

            _drawables = Array.Empty<ICube>();
        }
        else if (_drawables.Length < 2 && _sp is not null /*&& _dontCreate == false*/)
        {
            _drawables = new List<ICube>(_drawables)
                {
                    _sp.GetRequiredService<ICube>()
                }.ToArray();
            //_dontCreate = true;
        }

        //Share the rainbow
        _r = MathF.Sin((_sin + 0) * MathF.PI / 180);
        _g = MathF.Sin((_sin + 120) * MathF.PI / 180);
        _b = MathF.Sin((_sin + 240) * MathF.PI / 180);

        //Taste the rainbow
        if (++_sin > 360)
        {
            _sin = 0;
        }

        _drawables.ElementAtOrDefault(0)
            ?.Update(Vector3.Zero, _angleInRadians);
        _drawables.ElementAtOrDefault(1)
            ?.Update(new Vector3(0.5f, 0.0f, -0.5f), -_angleInRadians * 1.5f);

        return Task.CompletedTask;
    }

    /// <summary>
    /// https://bitbucket.org/snippets/DemoBytom/aejA59/maps-value-between-from-one-min-max-range
    /// </summary>
    public static float Map(float value, float inMin, float inMax, float outMin, float outMax) =>
        ((value - inMin) * (outMax - outMin) / (inMax - inMin)) + outMin;

    private float _angleInRadians = 0.0f;
    private ICube[] _drawables = Array.Empty<ICube>();
    private const float TWO_PI = MathHelper.TwoPi;

    private Task Render(IRenderingEngine renderingEngine)

    {
        _angleInRadians = (_angleInRadians + 0.01f) % TWO_PI;

        renderingEngine.BeginScene(new Color4(_r, _g, _b, 1.0f));
        renderingEngine.Draw(_drawables);
        _ = renderingEngine.EndScene();

        if (renderingEngine.Control.IsFullscreen != _fullscreen)
        {
            renderingEngine.SetFullscreen(_fullscreen);
        }

        return Task.CompletedTask;
    }

    #region IDisposable

    protected override void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _loopCancellationTokenSource.Dispose();
            }

            _disposedValue = true;
        }
        base.Dispose(disposing);
    }

    #endregion IDisposable
}