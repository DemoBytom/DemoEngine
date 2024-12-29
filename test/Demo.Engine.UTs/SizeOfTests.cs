// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.CompilerServices;
using Demo.Engine.Platform.DirectX12;
using FluentAssertions;

namespace Demo.Engine.UTs;

public class SizeOfTests
{
    [Fact]
    public void TestSizeOf()
    {
        var unsafeSizeOf = Unsafe.SizeOf<CubeFacesColors>();

        var sizeOf = SizeHelper.GetSize<CubeFacesColors>();

        _ = sizeOf.Should().Be(unsafeSizeOf);
    }
}