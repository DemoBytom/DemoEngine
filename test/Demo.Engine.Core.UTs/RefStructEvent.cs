// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Core.UTs;

public class RefStructEvent
{
    [Test]
    public void SomeTest()
    {
        var fooMock = Mock.Of<IFoo>(MockBehavior.Strict);
        Assert.Fail("Issue fixed, can remove this!");
    }

    public ref struct FooRefStruct
    {

    }

    public interface IFoo
    {
        event EventHandler<FooRefStruct> FooRefStructEvent;
    }
}