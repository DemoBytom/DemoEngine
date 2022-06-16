// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;

namespace Demo.Engine.Core.Interfaces.Rendering;

public interface ICube : IDrawable
{
    void Update(Vector3 position, float rotationAngleInRadians);
}