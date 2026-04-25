// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.ValueObjects;

namespace Demo.Engine.Core.Features.StaThread;

internal interface IStaThreadWriter
{
    Task<RenderingSurfaceId> CreateSurface(
        IRenderingEngine rendering,
        CancellationToken cancellationToken = default);
}