// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using Demo.Engine.Core.ValueObjects;
using Vortice.Mathematics;

namespace Demo.Engine.Core.Interfaces.Rendering;

public interface IRenderingEngine : IAsyncDisposable
{
    //IRenderingControl Control { get; }
    IReadOnlyCollection<IRenderingSurface> RenderingSurfaces { get; }

    bool TryGetRenderingSurface(
        RenderingSurfaceId renderingSurfaceId,
        [NotNullWhen(true)]
        out IRenderingSurface? renderingSurface);

    ValueTask<RenderingSurfaceId> CreateSurfaceAsync(CancellationToken cancellationToken = default);

    void Draw(RenderingSurfaceId renderingSurfaceId, IEnumerable<IDrawable> drawables);

    void Draw(Color4 color, RenderingSurfaceId renderingSurfaceId, IEnumerable<IDrawable> drawables);

    public void LogDebugMessages();

    public void SetFullscreen(bool fullscreen);
}