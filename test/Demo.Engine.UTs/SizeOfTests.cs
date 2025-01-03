// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.CompilerServices;
using Demo.Engine.Platform.DirectX12;
using Demo.Engine.Platform.DirectX12.Buffers;
using FluentAssertions;

namespace Demo.Engine.UTs;

public class SizeOfTests
{
    [Fact]
    public void TestSizeOf_CubeFacesColors()
    {
        var unsafeSizeOf = (uint)Unsafe.SizeOf<CubeFacesColors>();

        var sizeOf = SizeHelper.GetSize<CubeFacesColors>();

        _ = sizeOf.Should().Be(unsafeSizeOf);
    }

    [Fact]
    public void TestSizeOf_Vertex()
    {
        var unsafeSizeOf = (uint)Unsafe.SizeOf<Vertex>();
        var sizeOf = SizeHelper.GetSize<Vertex>();

        _ = sizeOf.Should().Be(unsafeSizeOf);
    }
}