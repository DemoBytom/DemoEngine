// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Requests.Keyboard;
using MediatR;
using Microsoft.Extensions.Hosting;

namespace Demo.Engine.Core.Services;

public class MainLoopService : IMainLoopService
{
    private readonly IMediator _mediator;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IOSMessageHandler _oSMessageHandler;
    private readonly IRenderingEngine _renderingEngine;
    private readonly IDebugLayerLogger _debugLayerLogger;
    private readonly IFpsTimer _fpsTimer;

    public bool IsRunning { get; private set; }

    public MainLoopService(
        IMediator mediator,
        IHostApplicationLifetime applicationLifetime,
        IOSMessageHandler oSMessageHandler,
        IRenderingEngine renderingEngine,
        IDebugLayerLogger debugLayerLogger,
        IFpsTimer fpsTimer)
    {
        _mediator = mediator;
        _applicationLifetime = applicationLifetime;
        _oSMessageHandler = oSMessageHandler;
        _renderingEngine = renderingEngine;
        _debugLayerLogger = debugLayerLogger;
        _fpsTimer = fpsTimer;
    }

    public async Task RunAsync(
        Func<IRenderingSurface, KeyboardHandle, KeyboardCharCache, ValueTask> updateCallback,
        Func<IRenderingEngine, Guid, ValueTask> renderCallback,
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
        var keyboardHandle = await _mediator.Send(new KeyboardHandleRequest(), CancellationToken.None);
        var keyboardCharCache = await _mediator.Send(new KeyboardCharCacheRequest(), CancellationToken.None);
        var doEventsOk = true;
        while (
            doEventsOk
            && !_applicationLifetime.ApplicationStopping.IsCancellationRequested
            && !cancellationToken.IsCancellationRequested
            )
        {
            IsRunning = true;

            foreach (var renderingSurface in _renderingEngine.RenderingSurfaces)
            {
                doEventsOk &= _oSMessageHandler.DoEvents(renderingSurface.RenderingControl);
                if (!doEventsOk)
                {
                    break;
                }

                await updateCallback(
                    renderingSurface,
                    keyboardHandle,
                    keyboardCharCache);

                _fpsTimer.Start();
                await renderCallback(
                    _renderingEngine,
                    renderingSurface.ID);
                _fpsTimer.Stop();
            }

            _debugLayerLogger.LogMessages();
        }
        IsRunning = false;
    }
}