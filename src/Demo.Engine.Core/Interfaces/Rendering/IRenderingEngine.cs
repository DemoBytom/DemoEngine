// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Vortice.Mathematics;

namespace Demo.Engine.Core.Interfaces.Rendering;

public interface IRenderingEngine : IDisposable
{
    //IRenderingControl Control { get; }
    IReadOnlyCollection<IRenderingSurface> RenderingSurfaces { get; }

    void CreateSurface();

    //void BeginScene(Guid renderingSurfaceId, Color4 color);

    //bool EndScene(Guid renderingSurfaceId);

    //void BeginScene(Guid renderingSurfaceId);

    void Draw(Guid renderingSurfaceId, IEnumerable<IDrawable> drawables);

    void Draw(Color4 color, Guid renderingSurfaceId, IEnumerable<IDrawable> drawables);

    public void LogDebugMessages();

    public void SetFullscreen(bool fullscreen);
}