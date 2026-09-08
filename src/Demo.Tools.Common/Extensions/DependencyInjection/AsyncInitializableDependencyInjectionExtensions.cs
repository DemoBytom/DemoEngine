// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using Demo.Tools.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Tools.Common.Extensions.DependencyInjection;

public static class AsyncInitializableDependencyInjectionExtensions
{
    /// <summary>
    /// Provides extension methods for adding async initializable services
    /// implementing <see cref="IAsyncInitializable{TService}"/> to an <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds a transient service of the type specified in <typeparamref name="TService"/>
        /// as a <see cref="IAsyncInitializable{TService}"/> with an
        /// implementation type specified in <typeparamref name="TImplementation"/> to the
        /// specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ServiceLifetime.Transient"/>
        /// <remarks>
        /// Service should be retreived using one of:
        /// <list type="bullet">
        /// <item>
        /// <see cref="GetServiceAsync{TService}(IServiceProvider, CancellationToken)"/>
        /// </item>
        /// <item>
        /// <see cref="GetRequireServiceAsync{TService}(IServiceProvider, CancellationToken)"/>
        /// </item>
        /// </list>
        /// </remarks>
        public IServiceCollection AddTransientAsyncInitializable<
                TService,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
            where TService : class
            where TImplementation : class, TService, IAsyncInitializable<TService>
            => services.AddTransient<IAsyncInitializable<TService>, TImplementation>();

        /// <summary>
        /// Adds a scoped service of the type specified in <typeparamref name="TService"/>
        /// as a <see cref="IAsyncInitializable{TService}"/> with an
        /// implementation type specified in <typeparamref name="TImplementation"/> to the
        /// specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ServiceLifetime.Scoped"/>
        /// <remarks>
        /// Service should be retreived using one of:
        /// <list type="bullet">
        /// <item>
        /// <see cref="GetServiceAsync{TService}(IServiceProvider, CancellationToken)"/>
        /// </item>
        /// <item>
        /// <see cref="GetRequireServiceAsync{TService}(IServiceProvider, CancellationToken)"/>
        /// </item>
        /// </list>
        /// </remarks>
        public IServiceCollection AddScopedAsyncInitializable<
                TService,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
            where TService : class
            where TImplementation : class, TService, IAsyncInitializable<TService>
            => services.AddScoped<IAsyncInitializable<TService>, TImplementation>();

        /// <summary>
        /// Adds a singleton service of the type specified in <typeparamref name="TService"/>
        /// as a <see cref="IAsyncInitializable{TService}"/> with an
        /// implementation type specified in <typeparamref name="TImplementation"/> to the
        /// specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ServiceLifetime.Singleton"/>
        /// <remarks>
        /// Service should be retreived using one of:
        /// <list type="bullet">
        /// <item>
        /// <see cref="GetServiceAsync{TService}(IServiceProvider, CancellationToken)"/>
        /// </item>
        /// <item>
        /// <see cref="GetRequireServiceAsync{TService}(IServiceProvider, CancellationToken)"/>
        /// </item>
        /// </list>
        /// </remarks>
        public IServiceCollection AddSingletonAsyncInitializable<
                TService,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
            where TService : class
            where TImplementation : class, TService, IAsyncInitializable<TService>
            => services.AddSingleton<IAsyncInitializable<TService>, TImplementation>();

        /// <summary>
        /// Adds a transient service of the type specified in <typeparamref name="TService"/>
        /// as a <see cref="IAsyncInitializable{TService}"/> with an
        /// implementation type specified in <typeparamref name="TImplementation"/> to the
        /// specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ServiceLifetime.Transient"/>
        /// <remarks>
        /// Service should be retreived using one of:
        /// <list type="bullet">
        /// <item>
        /// <see cref="GetServiceAsync{TService}(IServiceProvider, CancellationToken)"/>
        /// </item>
        /// <item>
        /// <see cref="GetRequireServiceAsync{TService}(IServiceProvider, CancellationToken)"/>
        /// </item>
        /// </list>
        /// </remarks>
        public IServiceCollection AddKeyedTransientAsyncInitializable<
                TService,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
                object? serviceKey)
            where TService : class
            where TImplementation : class, TService, IAsyncInitializable<TService>
            => services.AddKeyedTransient<IAsyncInitializable<TService>, TImplementation>(serviceKey);

        /// <summary>
        /// Adds a scoped service of the type specified in <typeparamref name="TService"/>
        /// as a <see cref="IAsyncInitializable{TService}"/> with an
        /// implementation type specified in <typeparamref name="TImplementation"/> to the
        /// specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ServiceLifetime.Scoped"/>
        /// <remarks>
        /// Service should be retreived using one of:
        /// <list type="bullet">
        /// <item>
        /// <see cref="GetServiceAsync{TService}(IServiceProvider, CancellationToken)"/>
        /// </item>
        /// <item>
        /// <see cref="GetRequireServiceAsync{TService}(IServiceProvider, CancellationToken)"/>
        /// </item>
        /// </list>
        /// </remarks>
        public IServiceCollection AddKeyedScopedAsyncInitializable<
                TService,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
                object? serviceKey)
            where TService : class
            where TImplementation : class, TService, IAsyncInitializable<TService>
            => services.AddKeyedScoped<IAsyncInitializable<TService>, TImplementation>(serviceKey);

        /// <summary>
        /// Adds a singleton service of the type specified in <typeparamref name="TService"/>
        /// as a <see cref="IAsyncInitializable{TService}"/> with an
        /// implementation type specified in <typeparamref name="TImplementation"/> to the
        /// specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="serviceKey">The <see cref="ServiceDescriptor.ServiceKey"/> of the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ServiceLifetime.Singleton"/>
        /// <remarks>
        /// Service should be retreived using one of:
        /// <list type="bullet">
        /// <item>
        /// <see cref="GetServiceAsync{TService}(IServiceProvider, CancellationToken)"/>
        /// </item>
        /// <item>
        /// <see cref="GetRequireServiceAsync{TService}(IServiceProvider, CancellationToken)"/>
        /// </item>
        /// </list>
        /// </remarks>
        public IServiceCollection AddKeyedSingletonAsyncInitializable<
                TService,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
                object? serviceKey)
            where TService : class
            where TImplementation : class, TService, IAsyncInitializable<TService>
            => services.AddKeyedSingleton<IAsyncInitializable<TService>, TImplementation>(serviceKey);
    }

    /// <summary>
    /// Provides extension methods for retrieving and initializing async initializable services
    /// implementing <see cref="IAsyncInitializable{TService}"/>.
    /// </summary>
    /// <typeparam name="TService">The type of service object to get.</typeparam>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to retrieve the service object from.</param>
    extension<TService>(IServiceProvider serviceProvider)
    {
        /// <summary>
        /// Get service of type <typeparamref name="TService"/> from the <see cref="IServiceProvider"/>
        /// and calls <see cref="IAsyncInitializable{TService}.InitializeAsync(CancellationToken)"/> on it.
        /// </summary>
        /// <returns>A service object of type <typeparamref name="TService"/>.</returns>
        /// <exception cref="InvalidOperationException">There is no service of type <typeparamref name="TService"/>.</exception>
        public async ValueTask<TService> GetRequireServiceAsync(
            CancellationToken cancellationToken = default)
            => await serviceProvider
                .GetRequiredService<IAsyncInitializable<TService>>()
                .InitializeAsync(cancellationToken);

        /// <summary>
        /// Get service of type <typeparamref name="TService"/> from the <see cref="IServiceProvider"/>
        /// and calls <see cref="IAsyncInitializable{TService}.InitializeAsync(CancellationToken)"/> on it.
        /// </summary>
        /// <returns>A service object of type <typeparamref name="TService"/> or null if there is no such service.</returns>
        public async ValueTask<TService?> GetServiceAsync(
            CancellationToken cancellationToken = default)
            => serviceProvider
                .GetService<IAsyncInitializable<TService>>()
                is IAsyncInitializable<TService> asyncInitializable
                    ? await asyncInitializable.InitializeAsync(cancellationToken)
                    : await ValueTask.FromResult<TService?>(default);

        /// <summary>
        /// Get service of type <typeparamref name="TService"/> from the <see cref="IServiceProvider"/>
        /// and calls <see cref="IAsyncInitializable{TService}.InitializeAsync(CancellationToken)"/> on it.
        /// </summary>
        /// <param name="serviceKey">An object that specifies the key of service object to get.</param>
        /// <returns>A service object of type <typeparamref name="TService"/>.</returns>
        /// <exception cref="System.InvalidOperationException">There is no service of type <typeparamref name="TService"/>.</exception>
        public async ValueTask<TService> GetRequiredKeyedServiceAsync(
            object? serviceKey,
            CancellationToken cancellationToken = default)
            => await serviceProvider
                .GetRequiredKeyedService<IAsyncInitializable<TService>>(
                    serviceKey)
                .InitializeAsync(cancellationToken);

        /// <summary>
        /// Get service of type <typeparamref name="TService"/> from the <see cref="IServiceProvider"/>
        /// and calls <see cref="IAsyncInitializable{TService}.InitializeAsync(CancellationToken)"/> on it.
        /// </summary>
        /// <param name="serviceKey">An object that specifies the key of service object to get.</param>
        /// <returns>A service object of type <typeparamref name="TService"/> or null if there is no such service.</returns>
        public async ValueTask<TService?> GetKeyedServiceAsync(
            object? serviceKey,
            CancellationToken cancellationToken = default)
            => serviceProvider
                .GetKeyedService<IAsyncInitializable<TService>>(serviceKey)
                is IAsyncInitializable<TService> asyncInitializable
                    ? await asyncInitializable.InitializeAsync(cancellationToken)
                    : await ValueTask.FromResult<TService?>(default);
    }
}