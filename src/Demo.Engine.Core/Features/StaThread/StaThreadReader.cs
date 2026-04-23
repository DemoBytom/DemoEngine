// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Threading.Channels;

namespace Demo.Engine.Core.Features.StaThread;

internal sealed class StaThreadReader(
    ChannelReader<StaThreadRequests> channelReader)
    : IStaThreadReader
{
    public async Task Invoke(
        Func<Func<CancellationToken, ValueTask<bool>>, CancellationToken, Task<bool>> callback,
        CancellationToken cancellationToken = default)
    {
        await foreach (var request in channelReader
            .ReadAllAsync(cancellationToken)
            .WithCancellation(cancellationToken))
        {
            _ = await callback(
                request.InvokeAsync,
                cancellationToken);
        }
    }
}