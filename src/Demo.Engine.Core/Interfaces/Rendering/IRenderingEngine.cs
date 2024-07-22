// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;
using Demo.Engine.Core.Interfaces.Platform;
using Vortice.Mathematics;

namespace Demo.Engine.Core.Interfaces.Rendering;

public interface IRenderingEngine : IDisposable
{
    IRenderingControl Control { get; }

    void BeginScene(Color4 color);

    bool EndScene();

    void BeginScene();

    void Draw(IEnumerable<IDrawable> drawables);

    /// <summary>
    /// Temporary untill we have a proper Camera class
    /// </summary>
    Matrix4x4 ViewProjectionMatrix { get; }

    public void LogDebugMessages();

    public void SetFullscreen(bool fullscreen);
}