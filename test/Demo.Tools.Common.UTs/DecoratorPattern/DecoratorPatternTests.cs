// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Tools.Common.DecoratorPattern;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Assertions.Should;
using TUnit.Assertions.Should.Extensions;

namespace Demo.Tools.Common.UTs.DecoratorPattern;

public class DecoratorPatternTests
{
    [Test]
    public async Task TestDecorator()
    {
        var services = new ServiceCollection();
        services.AddTransient<IFoo, Foo>();
        services.Decorate<IFoo, FooDecorator>();
        services.Decorate<IFoo, FooDecorator2>();

        FooDecorator2? fooDecorator = null;
        await using (var serviceProvider = services.BuildServiceProvider())
        {
            var fooService = serviceProvider.GetRequiredService<IFoo>();

            await fooService.Should().NotBeNull();
            await fooService.GetName().Should().BeEqualTo("FooDecoratorDecorator2");
            fooDecorator = (await fooService.Should().BeOfType(typeof(FooDecorator2))) as FooDecorator2;
        }

        await fooDecorator.Should().NotBeNull();
        await fooDecorator.Disposed.Should().BeTrue();
    }

    public interface IFoo
    {
        string GetName();
    }

    public class Foo
        : IFoo
    {
        public string GetName()
            => "Foo";
    }

    public class FooDecorator(IFoo foo)
        : IFoo
    {
        public string GetName()
            => $"{foo.GetName()}Decorator";
    }

    public class FooDecorator2(IFoo foo)
        : IFoo
        , IDisposable
    {
        public string GetName()
            => $"{foo.GetName()}Decorator2";

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}