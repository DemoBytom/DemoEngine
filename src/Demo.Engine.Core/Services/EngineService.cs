using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Demo.Engine.Core.Platform;
using Demo.Engine.Core.Requests.Keyboard;
using MediatR;
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
        private readonly IRenderingFormFactory _renderFormFactory;
        private readonly IMediator _mediator;

        public EngineService(
            IHostApplicationLifetime applicationLifetime,
            ILogger<EngineService> logger,
            IRenderingFormFactory renderFormFactory,
            IMediator mediator)
        {
            _applicationLifetime = applicationLifetime;
            _logger = logger;
            _renderFormFactory = renderFormFactory;
            _mediator = mediator;
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

        private async Task DoWork()
        {
            _logger.LogInformation("Engine working!");

            try
            {
                using var rf = _renderFormFactory.Create();
                //TODO just for testing purposes, neets to be moved out
                rf.Show();

                //TODO proper main loop instead of simple while
                while (
                    rf.DoEvents()
                    && !_stopRequested
                    && !_applicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    var sb = Stopwatch.StartNew();
                    var keyboardState = await _mediator.Send(new KeyboardStateRequest());
                    sb.Stop();
                    //if (_keyboard.KeyPressed((char)Keys.D1))
                    {
                        //_mediato
                        var str = keyboardState.GetString();
                        _logger.LogInformation("Keyboard query took {elapsed} ns", sb.ElapsedTicks / (double)Stopwatch.Frequency * 1_000_000_000);
                        if (!string.IsNullOrEmpty(str))
                        {
                            _logger.LogInformation("Read chars from buffer: {str}", str);
                        }
                    }
                    //Esc
                    if (keyboardState.GetKeyState((char)27))
                    {
                        _applicationLifetime.StopApplication();
                    }
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