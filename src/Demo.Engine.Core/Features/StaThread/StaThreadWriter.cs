// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Threading.Channels;
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
        CancellationToken cancellationToken = default)
    {
        var createSurfaceRequest = StaThreadRequests.CreateSurface();
        await _channelWriter.WriteAsync(
                createSurfaceRequest,
                cancellationToken);

        return await createSurfaceRequest.Invoked;
    }

    public async ValueTask<bool> DoEventsOk(
        RenderingSurfaceId renderingSurfaceId,
        CancellationToken cancellationToken = default)
    {
        var request = _doEventsOkPool.Get();

        try
        {
            request.Reset(renderingSurfaceId, cancellationToken);

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