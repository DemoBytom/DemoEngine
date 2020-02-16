using System;
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

namespace Demo.Engine.Core.Services
{
    public class EngineService : IHostedService, IDisposable
    {
        private bool _disposedValue;
        private Task? _executingTask;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private bool _stopRequested;
        private readonly ILogger<EngineService> _logger;

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

        private IMainLoopService? _mainLoop;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private async Task DoWork()
        {
            _logger.LogInformation("Engine working! v{version}", _version);
            try
            {
                using var scope = _scopeFactory.CreateScope();

                _mainLoop = scope.ServiceProvider.GetRequiredService<IMainLoopService>();

                await _mainLoop.RunAsync(
                    Update,
                    Render,
                    _cts.Token);
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
                _cts.Cancel();
            }

            return Task.CompletedTask;
        }

        private Task Render(IRenderingEngine renderingEngine) => Task.CompletedTask;

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

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _applicationLifetime.StopApplication();
                    _cts.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);

        #endregion IDisposable Support
    }
}