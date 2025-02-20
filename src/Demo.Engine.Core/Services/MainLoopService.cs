// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Requests.Keyboard;
using Demo.Engine.Core.ValueObjects;
using MediatR;
using Microsoft.Extensions.Hosting;

namespace Demo.Engine.Core.Services;

internal sealed class MainLoopService(
    IMediator mediator,
    IHostApplicationLifetime applicationLifetime,
    IOSMessageHandler oSMessageHandler,
    IRenderingEngine renderingEngine,
    IDebugLayerLogger debugLayerLogger,
    IFpsTimer fpsTimer,
    IShaderAsyncCompiler shaderCompiler)
    : IMainLoopService
{
    private readonly IMediator _mediator = mediator;
    private readonly IHostApplicationLifetime _applicationLifetime = applicationLifetime;
    private readonly IOSMessageHandler _oSMessageHandler = oSMessageHandler;
    private readonly IRenderingEngine _renderingEngine = renderingEngine;
    private readonly IDebugLayerLogger _debugLayerLogger = debugLayerLogger;
    private readonly IFpsTimer _fpsTimer = fpsTimer;
    private readonly IShaderAsyncCompiler _shaderCompiler = shaderCompiler;

    public bool IsRunning { get; private set; }

    public async Task RunAsync(
        Func<IRenderingSurface, KeyboardHandle, KeyboardCharCache, ValueTask> updateCallback,
        Func<IRenderingEngine, RenderingSurfaceId, ValueTask> renderCallback,
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

        var compileShaders = _shaderCompiler.CompileShaders(
            cancellationToken);

        _ = compileShaders.GetAwaiter().GetResult();

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

                _fpsTimer.StartRenderingTimer(renderingSurface.ID);
                await renderCallback(
                    _renderingEngine,
                    renderingSurface.ID);
                _fpsTimer.StopRenderingTimer(renderingSurface.ID);
            }

            _debugLayerLogger.LogMessages();
        }
        IsRunning = false;
    }
}