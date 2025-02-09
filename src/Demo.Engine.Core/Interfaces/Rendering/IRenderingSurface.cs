// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.ValueObjects;

namespace Demo.Engine.Core.Interfaces.Rendering;

public interface IRenderingSurface
{
    RenderingSurfaceId ID { get; }

    IRenderingControl RenderingControl { get; }

    /// <summary>
    /// Temporary untill we have a proper Camera class
    /// </summary>
    Matrix4x4 ViewProjectionMatrix { get; }
}