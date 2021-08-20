// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vertex
    {
        public Vertex(
            float x, float y, float z,
            byte r, byte g, byte b, byte a)
            : this(new Vector3(x, y, z), new Color(r, g, b, a)) { }

        public Vertex(Vector3 position, Color color) =>
            (Position, Color) = (position, color);

        public Vector3 Position { get; }
        public Color Color { get; }

        public static readonly int SizeInBytes = Unsafe.SizeOf<Vertex>();
        public static readonly int PositionSizeInBytes = Unsafe.SizeOf<Vector3>();
        public static readonly int ColorSizeInBytes = Unsafe.SizeOf<Color>();
    }
}