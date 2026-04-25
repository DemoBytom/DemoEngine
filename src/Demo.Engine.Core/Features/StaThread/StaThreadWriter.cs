// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Threading.Channels;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.ValueObjects;

namespace Demo.Engine.Core.Features.StaThread;

internal sealed class StaThreadWriter(
    ChannelWriter<StaThreadRequests> channelWriter)
    : IStaThreadWriter,
      IDisposable
{
    private readonly ChannelWriter<StaThreadRequests> _channelWriter = channelWriter;
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