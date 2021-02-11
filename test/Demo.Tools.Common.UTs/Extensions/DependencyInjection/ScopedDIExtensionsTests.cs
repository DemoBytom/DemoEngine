using System.Threading;
using Demo.Tools.Common.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Demo.Tools.Common.UTs.Extensions.DependencyInjection
{
    public class ScopedDIExtensionsTests
    {
        [Fact]
        public void AddScoped_Multiple_Interfaces_Control_Sample()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            var counter = new Counter();
            services.AddSingleton(_ => counter);

            // Act
            services.AddScoped<Bar>();
            services.AddScoped<Foo1, Bar>();
            services.AddScoped<Foo2, Bar>();

            //Assert
            var serviceProvider = services.BuildServiceProvider();
            var foo1 = serviceProvider.GetRequiredService<Foo1>();
            var foo2 = serviceProvider.GetRequiredService<Foo2>();
            var bar = serviceProvider.GetRequiredService<Bar>();

            foo1.Should().NotBeSameAs(bar);
            foo2.Should().NotBeSameAs(bar);
            foo2.Should().NotBeSameAs(foo1);

            counter.ConstructedNo.Should().Be(3);
        }

        [Fact]
        public void AddScoped_2_Interfaces_ExpectedBehavior()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            var counter = new Counter();
            services.AddSingleton(_ => counter);
            // Act
            services.AddScoped<Foo1, Foo2, Bar>();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var foo1 = serviceProvider.GetRequiredService<Foo1>();
            var foo2 = serviceProvider.GetRequiredService<Foo2>();
            var bar = serviceProvider.GetRequiredService<Bar>();

            foo1.Should().BeSameAs(bar);
            foo2.Should().BeSameAs(bar);
            counter.ConstructedNo.Should().Be(1);
        }

        public interface Foo1 { }

        public interface Foo2 { }

        public class Bar : Foo1, Foo2
        {
            public Bar(Counter counter) => counter.Tick();
        }

        public class Counter
        {
            private int _constructedNo;
            public int ConstructedNo => _constructedNo;

            public void Tick() =>
                Interlocked.Increment(ref _constructedNo);
        }
    }
}