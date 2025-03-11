// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.ValueObjects;

namespace Demo.Engine.Core.Interfaces;

internal interface IStaThreadWriter
{
    Task<RenderingSurfaceId> CreateSurface(
        CancellationToken cancellationToken = default);

    ValueTask<bool> DoEventsOk(
        RenderingSurfaceId renderingSurfaceId,
        CancellationToken cancellationToken = default);
}