using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vortice.Mathematics;

namespace Demo.Engine.Core.Services
{
    public class EngineService : IHostedService, IDisposable
    {
        private bool _disposedValue;
        private Task? _executingTask;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private bool _stopRequested;
        private readonly ILogger<EngineService> _logger;
        private readonly CancellationTokenSource _loopCancellationTokenSource = new CancellationTokenSource();

        private readonly string _version =
            Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        private readonly IServiceScopeFactory _scopeFactory;

        public EngineService(
            IHostApplicationLifetime applicationLifetime,
            ILogger<EngineService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _applicationLifetime = applicationLifetime;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Engine starting! v{version}", _version);
            _executingTask = DoWorkAsync();
            return _executingTask.IsCompleted
                ? _executingTask
                : Task.CompletedTask;
        }

        private Task DoWorkAsync()
        {
            //Starts the work one a new STA thread so that Windows.Forms work correctly
            var tcs = new TaskCompletionSource<object?>();
            var thread = new Thread(async () =>
            {
                try
                {
                    await DoWork();
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Engine failed with error! {errorMessage}", ex.Message);
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

        private async Task DoWork()
        {
            _logger.LogInformation("Engine working! v{version}", _version);
            try
            {
                using var scope = _scopeFactory.CreateScope();

                _drawables = new[]
                {
                    scope.ServiceProvider.GetRequiredService<ICube>(),
                    scope.ServiceProvider.GetRequiredService<ICube>()
                };
                var mainLoop = scope.ServiceProvider.GetRequiredService<IMainLoopService>();

                await mainLoop.RunAsync(
                    Update,
                    Render,
                    _loopCancellationTokenSource.Token);
            }
            finally
            {
                //If StopAsync was already called, no reason to call it again
                if (!_stopRequested)
                {
                    _applicationLifetime.StopApplication();
                }
            }
        }

        private float _r, _g, _b = 0.0f;

        private float _sin = 0.0f;

        private Task Update(
            KeyboardHandle keyboardHandle,
            KeyboardCharCache keyboardCharCache)
        {
            if (keyboardHandle?.GetKeyPressed(VirtualKeys.OemOpenBrackets) == true)
            {
                keyboardCharCache.Clear();
            }
            if (keyboardHandle?.GetKeyPressed(VirtualKeys.OemCloseBrackets) == true)
            {
                var str = keyboardCharCache?.ReadCache();
                if (!string.IsNullOrEmpty(str))
                {
                    _logger.LogInformation(str);
                }
            }
            if (keyboardHandle?.GetKeyPressed(VirtualKeys.Escape) == true)
            {
                _loopCancellationTokenSource.Cancel();
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
            renderingEngine.EndScene();

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Engine stopping!");

            _stopRequested = true;
            if (_executingTask == null)
            {
                return;
            }

            await Task.WhenAny(
                _executingTask,
                Task.Delay(Timeout.Infinite, cancellationToken));
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _applicationLifetime.StopApplication();
                    _loopCancellationTokenSource.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);

        #endregion IDisposable
    }
}