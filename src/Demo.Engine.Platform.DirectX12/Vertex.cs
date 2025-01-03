// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using Demo.Engine.Platform.DirectX12.Buffers;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX12;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal readonly record struct Vertex(
    Vector3 Position,
    Color Color)
    : ISizeInBytes<Vertex>
{
    public static unsafe uint SizeInBytes { get; } = (uint)sizeof(Vertex);
    public static unsafe uint PositionSizeInBytes { get; } = (uint)sizeof(Vector3);
    public static unsafe uint ColorSizeInBytes { get; } = (uint)sizeof(Color);
}