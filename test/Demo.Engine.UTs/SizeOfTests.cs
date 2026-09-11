// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.CompilerServices;
using Demo.Engine.Platform.DirectX12;
using Demo.Engine.Platform.DirectX12.Buffers;
using TUnit.Assertions.Should;
using TUnit.Assertions.Should.Extensions;

namespace Demo.Engine.UTs;

public class SizeOfTests
{
    [Test]
    public async Task TestSizeOf_CubeFacesColors()
    {
        var unsafeSizeOf = (uint)Unsafe.SizeOf<CubeFacesColors>();

        var sizeOf = SizeHelper.GetSize<CubeFacesColors>();

        await sizeOf.Should().BeEqualTo(unsafeSizeOf);
    }

    [Test]
    public async Task TestSizeOf_Vertex()
    {
        var unsafeSizeOf = (uint)Unsafe.SizeOf<Vertex>();
        var sizeOf = SizeHelper.GetSize<Vertex>();

        await sizeOf.Should().BeEqualTo(unsafeSizeOf);
    }
}