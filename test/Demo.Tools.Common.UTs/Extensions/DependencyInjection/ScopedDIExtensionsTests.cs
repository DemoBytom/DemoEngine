// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Tools.Common.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Assertions.Should;
using TUnit.Assertions.Should.Extensions;

namespace Demo.Tools.Common.UTs.Extensions.DependencyInjection;

public class ScopedDIExtensionsTests
{
    [Test]
    public async Task AddScoped_Multiple_Interfaces_Control_SampleAsync()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        var counter = new Counter();
        _ = services.AddSingleton(_ => counter);

        // Act
        _ = services.AddScoped<Bar>();
        _ = services.AddScoped<Foo1, Bar>();
        _ = services.AddScoped<Foo2, Bar>();

        //Assert
        var serviceProvider = services.BuildServiceProvider();
        var foo1 = serviceProvider.GetRequiredService<Foo1>();
        var foo2 = serviceProvider.GetRequiredService<Foo2>();
        var bar = serviceProvider.GetRequiredService<Bar>();

        await foo1.Should().NotBeSameReferenceAs(bar);
        await foo2.Should().NotBeSameReferenceAs(bar);
        await foo2.Should().NotBeSameReferenceAs(foo1);// .ShouldNotBeSameAs(foo1);

        await counter.ConstructedNo.Should().BeEqualTo(3);
    }

    [Test]
    public async Task AddScoped_2_Interfaces_ExpectedBehavior()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        var counter = new Counter();
        _ = services.AddSingleton(_ => counter);
        // Act
        _ = services.AddScoped<Foo1, Foo2, Bar>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var foo1 = serviceProvider.GetRequiredService<Foo1>();
        var foo2 = serviceProvider.GetRequiredService<Foo2>();
        var bar = serviceProvider.GetRequiredService<Bar>();

        await foo1.Should().BeSameReferenceAs(bar);
        await foo2.Should().BeSameReferenceAs(bar);
        await counter.ConstructedNo.Should().BeEqualTo(1);
    }

    public interface Foo1
    { }

    public interface Foo2
    { }

    public class Bar : Foo1, Foo2
    {
        public Bar(Counter counter) => counter.Tick();
    }

    public class Counter
    {
        private int _constructedNo;
        public int ConstructedNo => _constructedNo;

        public void Tick()
            => Interlocked.Increment(ref _constructedNo);
    }
}