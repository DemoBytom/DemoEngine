// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.InteropServices;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX12;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public readonly record struct CubeFacesColors(
    Color4 Face1,
    Color4 Face2,
    Color4 Face3,
    Color4 Face4,
    Color4 Face5,
    Color4 Face6)
    : ISizeInBytes<CubeFacesColors>
{
    public static unsafe int SizeInBytes => sizeof(CubeFacesColors);
}

public interface ISizeInBytes<TSelf>
    where TSelf : unmanaged, ISizeInBytes<TSelf>
{
    public static abstract int SizeInBytes { get; }
}

public static class SizeHelper
{
    public static int GetSize<T>()
        where T : unmanaged, ISizeInBytes<T>
        => T.SizeInBytes;
}