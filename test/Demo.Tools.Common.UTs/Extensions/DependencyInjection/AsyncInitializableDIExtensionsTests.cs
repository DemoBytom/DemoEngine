// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Tools.Common.Extensions.DependencyInjection;
using Demo.Tools.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Assertions.Should;
using TUnit.Assertions.Should.Extensions;

namespace Demo.Tools.Common.UTs.Extensions.DependencyInjection;

public class AsyncInitializableDIExtensionsTests
{
    [Test]
    [Arguments(ServiceLifetime.Transient, true, null)]
    [Arguments(ServiceLifetime.Transient, false, null)]
    [Arguments(ServiceLifetime.Scoped, true, null)]
    [Arguments(ServiceLifetime.Scoped, false, null)]
    [Arguments(ServiceLifetime.Singleton, true, null)]
    [Arguments(ServiceLifetime.Singleton, false, null)]
    [Arguments(ServiceLifetime.Transient, true, "serviceKey")]
    [Arguments(ServiceLifetime.Transient, false, "serviceKey")]
    [Arguments(ServiceLifetime.Scoped, true, "serviceKey")]
    [Arguments(ServiceLifetime.Scoped, false, "serviceKey")]
    [Arguments(ServiceLifetime.Singleton, true, "serviceKey")]
    [Arguments(ServiceLifetime.Singleton, false, "serviceKey")]
    public async Task RetreivedService_Should_Be_Initialized(
        ServiceLifetime serviceLifetime,
        bool required,
        object? serviceKey)
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddServiceFunc<IAsyncFoo, AsyncFoo>(serviceLifetime, serviceKey);

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        // Act
        var asyncInitializable = await scope.ServiceProvider.GetServiceFunc<IAsyncFoo>(required, serviceKey);

        // Assert
        await asyncInitializable.Initialized.Should().BeTrue();
    }

    [Test]
    [Arguments(true, null)]
    [Arguments(false, null)]
    [Arguments(true, "serviceKey")]
    [Arguments(false, "serviceKey")]
    public async Task Transient_Service_Should_Have_Different_IDs(
        bool required,
        object? serviceKey)
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddServiceFunc<IAsyncFoo, AsyncFoo>(ServiceLifetime.Transient, serviceKey);
        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        // Act
        var asyncInitializable1 = await scope.ServiceProvider.GetServiceFunc<IAsyncFoo>(required, serviceKey);
        var asyncInitializable2 = await scope.ServiceProvider.GetServiceFunc<IAsyncFoo>(required, serviceKey);

        // Assert
        await asyncInitializable1.ID.Should().NotBeEqualTo(asyncInitializable2.ID);
    }

    [Test]
    [Arguments(ServiceLifetime.Scoped, true, null)]
    [Arguments(ServiceLifetime.Scoped, false, null)]
    [Arguments(ServiceLifetime.Singleton, true, null)]
    [Arguments(ServiceLifetime.Singleton, false, null)]
    [Arguments(ServiceLifetime.Scoped, true, "serviceKey")]
    [Arguments(ServiceLifetime.Scoped, false, "serviceKey")]
    [Arguments(ServiceLifetime.Singleton, true, "serviceKey")]
    [Arguments(ServiceLifetime.Singleton, false, "serviceKey")]
    public async Task Scoped_Singleton_Should_Have_Same_IDs_In_Scope(
        ServiceLifetime serviceLifetime,
        bool required,
        object? serviceKey)
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddServiceFunc<IAsyncFoo, AsyncFoo>(serviceLifetime, serviceKey);

        // Act
        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        var asyncInitializable1 = await scope.ServiceProvider.GetServiceFunc<IAsyncFoo>(required, serviceKey);
        var asyncInitializable2 = await scope.ServiceProvider.GetServiceFunc<IAsyncFoo>(required, serviceKey);

        // Assert
        await asyncInitializable1.ID.Should().BeEqualTo(asyncInitializable2.ID);
    }

    private sealed class AsyncFoo
        : IAsyncInitializable<IAsyncFoo>
        , IAsyncFoo
    {
        public Guid ID { get; } = Guid.NewGuid();
        public bool Initialized { get; private set; }

        public async ValueTask<IAsyncFoo> InitializeAsync
            (CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken);

            Initialized = true;
            return this;
        }
    }

    private interface IAsyncFoo
    {
        bool Initialized { get; }
        Guid ID { get; }
    }
}

static file class AsyncInitializableDIExtensionsTestsExtensions
{
    extension<TService>(IServiceProvider sp)
    {
        public async ValueTask<TService> GetServiceFunc(
            bool required,
            object? serviceKey)
       => (required, serviceKey) switch
       {
           (true, null) => await sp.GetRequireServiceAsync<TService>(),
           (true, not null) => await sp.GetRequiredKeyedServiceAsync<TService>(serviceKey),
           (false, null) => (await sp.GetServiceAsync<TService>())!,
           (false, not null) => (await sp.GetKeyedServiceAsync<TService>(serviceKey))!,
       };
    }

    extension<TService, TImplementation>(IServiceCollection services)
        where TService : class
        where TImplementation : class, TService, IAsyncInitializable<TService>
    {
        public IServiceCollection AddServiceFunc(
            ServiceLifetime lifetime,
            object? serviceKey)
        => (lifetime, serviceKey) switch
        {
            (ServiceLifetime.Singleton, null) => services.AddSingletonAsyncInitializable<TService, TImplementation>(),
            (ServiceLifetime.Singleton, not null) => services.AddKeyedSingletonAsyncInitializable<TService, TImplementation>(serviceKey),
            (ServiceLifetime.Scoped, null) => services.AddScopedAsyncInitializable<TService, TImplementation>(),
            (ServiceLifetime.Scoped, not null) => services.AddKeyedScopedAsyncInitializable<TService, TImplementation>(serviceKey),
            (ServiceLifetime.Transient, null) => services.AddTransientAsyncInitializable<TService, TImplementation>(),
            (ServiceLifetime.Transient, not null) => services.AddKeyedTransientAsyncInitializable<TService, TImplementation>(serviceKey),
            _ => throw new NotSupportedException($"Service lifetime {lifetime} is not supported."),
        };
    }
}