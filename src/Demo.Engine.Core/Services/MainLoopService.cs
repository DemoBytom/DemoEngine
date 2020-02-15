using System.Threading.Tasks;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Microsoft.Extensions.Hosting;

namespace Demo.Engine.Core.Services
{
    public class MainLoopService : IMainLoopService
    {
        private readonly IHostApplicationLifetime _applicationLifetime;
        private bool _stopRequested;
        private readonly IOSMessageHandler _oSMessageHandler;
        private readonly IRenderingEngine _renderingEngine;

        public bool IsRunning { get; private set; }

        public MainLoopService(
            IHostApplicationLifetime applicationLifetime,
            IOSMessageHandler oSMessageHandler,
            IRenderingEngine renderingEngine)
        {
            _applicationLifetime = applicationLifetime;
            _oSMessageHandler = oSMessageHandler;
            _renderingEngine = renderingEngine;
        }

        public async Task RunAsync(
            UpdateCallback updateCallback,
            RenderCallback renderCallback)
        {
            //TODO proper main loop instead of simple while
            while (
                _oSMessageHandler.DoEvents(_renderingEngine.Control)
                && !_applicationLifetime.ApplicationStopping.IsCancellationRequested
                && !_stopRequested
                )
            {
                IsRunning = true;
                await updateCallback();
                await renderCallback();
            }
            IsRunning = false;
        }

        public void Stop() => _stopRequested = true;

        public delegate Task RenderCallback();

        public delegate Task UpdateCallback();
    }
}