// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Core.ValueObjects;
public readonly record struct RenderingSurfaceId(
    Guid Value)
{
    public static RenderingSurfaceId NewId()
        => new(
            Guid.CreateVersion7());

    public static RenderingSurfaceId Empty { get; } = new(Guid.Empty);

    public override string ToString()
        => Value.ToString();

    public override int GetHashCode()
        => Value.GetHashCode();
}