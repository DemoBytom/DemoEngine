// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.ValueObjects;

namespace Demo.Engine.Platform.DirectX12.ForwardPlusRenderer;

public readonly struct FrameInfo
{
    public required Width Width { get; init; }
    public required Height Height { get; init; }
    public required uint UPS_Frame { get; init; }
}