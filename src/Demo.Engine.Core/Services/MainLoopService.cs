using System;
using System.Threading;
using System.Threading.Tasks;
using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Requests.Keyboard;
using MediatR;
using Microsoft.Extensions.Hosting;

namespace Demo.Engine.Core.Services
{
    public class MainLoopService : IMainLoopService
    {
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IOSMessageHandler _oSMessageHandler;
        private readonly IRenderingEngine _renderingEngine;
        private readonly IMediator _mediator;

        public bool IsRunning { get; private set; }

        public MainLoopService(
            IMediator mediator,
            IHostApplicationLifetime applicationLifetime,
            IOSMessageHandler oSMessageHandler,
            IRenderingEngine renderingEngine)
        {
            _applicationLifetime = applicationLifetime;
            _oSMessageHandler = oSMessageHandler;
            _renderingEngine = renderingEngine;
            _mediator = mediator;
        }

        public async Task RunAsync(
            Func<KeyboardHandle, KeyboardCharCache, Task> updateCallback,
            Func<IRenderingEngine, Task> renderCallback,
            CancellationToken cancellationToken = default)
        {
            if (updateCallback is null)
            {
                throw new ArgumentNullException(nameof(updateCallback), "Update callback method cannot be null!");
            }
            if (renderCallback is null)
            {
                throw new ArgumentNullException(nameof(renderCallback), "Render callback method cannot be null!");
            }
            //TODO proper main loop instead of simple while
            var keyboardHandle = await _mediator.Send(new KeyboardHandleRequest());
            var keyboardCharCache = await _mediator.Send(new KeyboardCharCacheRequest());

            while (
                _oSMessageHandler.DoEvents(_renderingEngine.Control)
                && !_applicationLifetime.ApplicationStopping.IsCancellationRequested
                && !cancellationToken.IsCancellationRequested
                )
            {
                IsRunning = true;
                await updateCallback(
                    keyboardHandle,
                    keyboardCharCache);
                await renderCallback(_renderingEngine);
            }
            IsRunning = false;
        }
    }
}