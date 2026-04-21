// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.ValueObjects;

namespace Demo.Engine.Core.Features.StaThread;

internal interface IStaThreadWriter
{
    Task<RenderingSurfaceId> CreateSurface(
        IRenderingEngine rendering,
        CancellationToken cancellationToken = default);

    ValueTask<bool> DoEventsOk(
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler,
        RenderingSurfaceId renderingSurfaceId,
        CancellationToken cancellationToken = default);

    ValueTask<bool> BlockingDoEventsOk(
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler,
        RenderingSurfaceId renderingSurfaceId,
        CancellationToken cancellationToken = default);
}