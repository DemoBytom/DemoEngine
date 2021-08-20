using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Core.Services
{
    public abstract class ServiceBase : IHostedService, IDisposable
    {
        private bool _disposedValue;
        protected readonly ILogger _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private Task? _executingTask;
        private bool _stopRequested;

        protected readonly string _version = Assembly
            .GetEntryAssembly()
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "0.0.0";

        private readonly string _serviceName;

        protected ServiceBase(
            string serviceName,
            ILogger logger,
            IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _serviceName = serviceName;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("{serviceName} starting! v{version}", _serviceName, _version);
            _executingTask = DoWorkAsync();

            //If task is completed return it,
            //otherwise return CompletedTask since DoWork is running
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
                    _logger.LogCritical(ex, "{serviceName} failed with error! {errorMessage}", _serviceName, ex.Message);
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
            _logger.LogInformation("{serviceName} working! v{version}", _serviceName, _version);
            try
            {
                await ExecuteAsync();
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

        protected abstract Task ExecuteAsync();

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _applicationLifetime.StopApplication();
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

        #endregion IDisposable
    }
}