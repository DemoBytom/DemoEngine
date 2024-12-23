// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX12;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vertex(
    Vector3 position,
    Color color)
{
    public Vector3 Position { get; } = position;
    public Color Color { get; } = color;

    public static readonly uint SizeInBytes = (uint)Unsafe.SizeOf<Vertex>();
    public static readonly uint PositionSizeInBytes = (uint)Unsafe.SizeOf<Vector3>();
    public static readonly uint ColorSizeInBytes = (uint)Unsafe.SizeOf<Color>();
}