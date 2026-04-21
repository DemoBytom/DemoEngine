// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Threading.Channels;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.ValueObjects;
using Microsoft.Extensions.ObjectPool;

namespace Demo.Engine.Core.Features.StaThread;

internal sealed class StaThreadWriter(
    ChannelWriter<StaThreadRequests> channelWriter,
    ObjectPool<StaThreadRequests.DoEventsOkRequest> doEventsOkPool)
    : IStaThreadWriter,
      IDisposable
{
    private readonly ChannelWriter<StaThreadRequests> _channelWriter = channelWriter;
    private readonly ObjectPool<StaThreadRequests.DoEventsOkRequest> _doEventsOkPool = doEventsOkPool;
    private bool _disposedValue;

    public async Task<RenderingSurfaceId> CreateSurface(
        IRenderingEngine renderingEngine,
        CancellationToken cancellationToken = default)
    {
        var createSurfaceRequest = StaThreadRequests.CreateSurface(renderingEngine);
        await _channelWriter.WriteAsync(
                createSurfaceRequest,
                cancellationToken);

        return await createSurfaceRequest.Invoked;
    }

    public async ValueTask<bool> DoEventsOk(
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler,
        RenderingSurfaceId renderingSurfaceId,
        CancellationToken cancellationToken = default)
    {
        var request = _doEventsOkPool.Get();

        try
        {
            request.Reset(renderingEngine, osMessageHandler, renderingSurfaceId, blockingCall: false, cancellationToken);

            await _channelWriter.WriteAsync(
                request,
                cancellationToken);

            return await request.Invoked;
        }
        finally
        {
            _doEventsOkPool.Return(request);
        }
    }

    public async ValueTask<bool> BlockingDoEventsOk(
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler,
        RenderingSurfaceId renderingSurfaceId,
        CancellationToken cancellationToken = default)
    {
        var request = _doEventsOkPool.Get();

        try
        {
            request.Reset(renderingEngine, osMessageHandler, renderingSurfaceId, blockingCall: true, cancellationToken);

            await _channelWriter.WriteAsync(
                request,
                cancellationToken);

            return await request.Invoked;
        }
        finally
        {
            _doEventsOkPool.Return(request);
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _channelWriter.Complete();
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
}