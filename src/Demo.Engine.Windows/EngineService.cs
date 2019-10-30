using System;
using System.Threading;
using System.Threading.Tasks;
using Demo.Engine.Windows.Platform;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Windows
{
    public class EngineService : IHostedService, IDisposable
    {
        private bool _disposedValue;
        private Task? _executingTask;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private bool _stopRequested;
        private readonly ILogger<EngineService> _logger;
        private readonly IRenderingFormFactory _renderFormFactory;

        public EngineService(
            IHostApplicationLifetime applicationLifetime,
            ILogger<EngineService> logger,
            IRenderingFormFactory renderFormFactory)
        {
            _applicationLifetime = applicationLifetime;
            _logger = logger;
            _renderFormFactory = renderFormFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Engine starting!");
            _executingTask = DoWorkAsync();
            return _executingTask.IsCompleted
                ? _executingTask
                : Task.CompletedTask;
        }

        private Task DoWorkAsync()
        {
            //Starts the work one a new STA thread so that Windows.Forms work correctly
            var tcs = new TaskCompletionSource<object?>();
            var thread = new Thread(() =>
            {
                try
                {
                    DoWork();
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        private void DoWork()
        {
            _logger.LogInformation("Engine working!");

            try
            {
                using var rf = _renderFormFactory.Create();
                rf.Show();

                //TODO proper main loop instead of simple while
                while (
                    rf.DoEvents()
                    && !_stopRequested
                    && !_applicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    //app runs
                }
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
                    //_renderControl?.Dispose();
                    _applicationLifetime.StopApplication();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}