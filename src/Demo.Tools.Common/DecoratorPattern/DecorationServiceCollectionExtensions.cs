// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Tools.Common.DecoratorPattern;

// source: https://github.com/khellang/Scrutor/blob/7f315dab5b0f7134a6be58941db1da8c904507e2/src/Scrutor/ServiceCollectionExtensions.Decoration.cs
public static class DecorationServiceCollectionExtensions
{
    private const string DECORATED_SERVICE_KEY_SUFFIX = "+Decorated";

    public static IServiceCollection Decorate<TService, TDecorator>(
        this IServiceCollection services)
        where TDecorator : TService
    {
        return services.Decorate<TService, TDecorator>(out _);
    }

    public static IServiceCollection Decorate<TService, TDecorator>(
        this IServiceCollection services,
        out DecoratedService<TService> decorated)
        where TDecorator : TService
    {
        services = services.Decorate(typeof(TService), typeof(TDecorator), out var decoratedObj);
        decorated = decoratedObj.Downcast<TService>();
        return services;
    }

    public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Type decoratorType, out DecoratedService<object> decorated)
    {
        return services.Decorate(
            DecorationStrategy.WithType(
                serviceType,
                serviceKey: null,
                decoratorType),
            out decorated);
    }

    public static IServiceCollection Decorate(this IServiceCollection services, DecorationStrategy strategy, out DecoratedService<object> decorated)
    {
        if (services.TryDecorate(strategy, out decorated!))
        {
            return services;
        }

        //throw new DecorationException(strategy);
        throw new Exception("TODO exception!");
    }

    public static bool TryDecorate(
        this IServiceCollection services,
        DecorationStrategy decorationStrategy,
        [NotNullWhen(true)] out DecoratedService<object>? decoratedService)
    {
        var decoratedKeys = new List<string>();

        for (var i = services.Count - 1; i >= 0; --i)
        {
            var serviceDescriptor = services[i];

            if (serviceDescriptor.IsDecorated()
                || !decorationStrategy.CanDecorate(serviceDescriptor))
            {
                continue;
            }

            var serviceKey = serviceDescriptor.GetDecoratorKey();
            if (serviceKey is null)
            {
                decoratedService = null;
                return false;
            }

            // Insert decorated
            services.Add(serviceDescriptor.WithServiceKey(serviceKey));
            decoratedKeys.Add(serviceKey);

            // Replace decorator
            services[i] = serviceDescriptor.WithImplementationFactory(
                decorationStrategy.CreateDecorator(
                    serviceDescriptor.ServiceType,
                    serviceKey));
        }

        decoratedService = new DecoratedService<object>(
            decorationStrategy.ServiceType,
            decoratedKeys);

        return decoratedKeys.Count > 0;
    }

    private static bool IsDecorated(
        this ServiceDescriptor serviceDescriptor)
        => serviceDescriptor.ServiceKey is string stringKey
        && stringKey.EndsWith(DECORATED_SERVICE_KEY_SUFFIX, StringComparison.Ordinal)
        ;

    private static string? GetDecoratorKey(
        this ServiceDescriptor descriptor)
    {
        var uniqueKey = Guid.CreateVersion7().ToString("n");

        return descriptor.ServiceKey switch
        {
            null
                => $"{descriptor.ServiceType.Name}+{uniqueKey}{DECORATED_SERVICE_KEY_SUFFIX}",
            string stringKey
                => $"{stringKey}+{uniqueKey}{DECORATED_SERVICE_KEY_SUFFIX}",
            _
                => null,
        };
    }
}

// source: https://github.com/khellang/Scrutor/blob/7f315dab5b0f7134a6be58941db1da8c904507e2/src/Scrutor/ServiceDescriptorExtensions.cs
internal static class ServiceDescriptorExtensions
{
    public static ServiceDescriptor WithImplementationFactory(
        this ServiceDescriptor descriptor,
        Func<IServiceProvider, object?, object> implementationFactory)
        => new(
            descriptor.ServiceType,
            descriptor.ServiceKey,
            implementationFactory,
            descriptor.Lifetime);

    public static ServiceDescriptor WithServiceKey(
        this ServiceDescriptor serviceDescriptor,
        string serviceKey)
        => serviceDescriptor.IsKeyedService
        ? ReplaceServiceKey(serviceDescriptor, serviceKey)
        : AddServiceKey(serviceDescriptor, serviceKey);

    private static ServiceDescriptor ReplaceServiceKey(
        ServiceDescriptor serviceDescriptor,
        string serviceKey)
        => serviceDescriptor switch
        {
            { KeyedImplementationType: not null }
                => new(
                    serviceDescriptor.ServiceType,
                    serviceKey,
                    serviceDescriptor.KeyedImplementationType,
                    serviceDescriptor.Lifetime),
            { KeyedImplementationFactory: not null }
                => new(
                    serviceDescriptor.ServiceType,
                    serviceKey,
                    serviceDescriptor.KeyedImplementationFactory,
                    serviceDescriptor.Lifetime),
            { KeyedImplementationInstance: not null }
                => new(
                    serviceDescriptor.ServiceType,
                    serviceKey,
                    serviceDescriptor.KeyedImplementationInstance),
            _
                => throw new ArgumentException(
                    message: $"No implemetation factory or instance or type found for {serviceDescriptor.ServiceType}!",
                    paramName: nameof(serviceDescriptor)),
        };

    private static ServiceDescriptor AddServiceKey(
        ServiceDescriptor serviceDescriptor,
        string serviceKey)
        => serviceDescriptor switch
        {
            { ImplementationType: not null }
                => new(
                    serviceDescriptor.ServiceType,
                    serviceKey,
                    serviceDescriptor.ImplementationType,
                    serviceDescriptor.Lifetime),
            { ImplementationFactory: not null }
                => new(
                    serviceDescriptor.ServiceType,
                    serviceKey,
                    DiscardServiceKey(serviceDescriptor.ImplementationFactory),
                    serviceDescriptor.Lifetime),
            { ImplementationInstance: not null }
                => new(
                    serviceDescriptor.ServiceType,
                    serviceKey,
                    serviceDescriptor.ImplementationInstance),
            _
                => throw new ArgumentException(
                    message: $"No implementation factory or instance or type found for {serviceDescriptor.ServiceType}.",
                    paramName: nameof(serviceDescriptor))
        };

    private static Func<IServiceProvider, object?, object> DiscardServiceKey(
        Func<IServiceProvider, object> factory)
        => (sp, key) => factory(sp);
}

// source: https://github.com/khellang/Scrutor/blob/7f315dab5b0f7134a6be58941db1da8c904507e2/src/Scrutor/DecorationStrategy.cs
public abstract class DecorationStrategy(
    Type serviceType,
    string? serviceKey)
{
    public Type ServiceType { get; } = serviceType;
    public string? ServiceKey { get; } = serviceKey;

    public virtual bool CanDecorate(
        ServiceDescriptor descriptor)
        // object.Equals is used to support decorating services with object keys
        // (e.g., KeyedService.AnyKey).
        => Equals(ServiceKey, descriptor.ServiceKey)
        && CanDecorate(descriptor.ServiceType);

    protected abstract bool CanDecorate(
        Type serviceType);

    public abstract Func<IServiceProvider, object?, object> CreateDecorator(
        Type serviceType,
        string serviceKey);

    internal static DecorationStrategy WithType(
        Type serviceType,
        string? serviceKey,
        Type decoratorType)
        => Create(serviceType, serviceKey, decoratorType, decoratorFactory: null);

    protected static Func<IServiceProvider, object?, object> TypeDecorator(
        Type serviceType,
        string serviceKey,
        Type decoratorType)
        => (sp, _) =>
        {
            var instanceToDecorate = sp.GetRequiredKeyedService(serviceType, serviceKey);
            return ActivatorUtilities.CreateInstance(sp, decoratorType, instanceToDecorate);
        };

    protected static Func<IServiceProvider, object?, object> FactoryDecorator(
        Type serviceType,
        string serviceKey,
        Func<object, IServiceProvider, object> decoratorFactory)
        => (sp, _) =>
        {
            var instanceToDecorate = sp.GetRequiredKeyedService(serviceType, serviceKey);
            return decoratorFactory(instanceToDecorate, sp);
        };

    private static DecorationStrategy Create(Type serviceType, string? serviceKey, Type? decoratorType, Func<object, IServiceProvider, object>? decoratorFactory)
    {
        //if (serviceType.IsOpenGeneric())
        //{
        //    return new OpenGenericDecorationStrategy(serviceType, serviceKey, decoratorType, decoratorFactory);
        //}

        return new ClosedTypeDecorationStrategy(
            serviceType,
            serviceKey,
            decoratorType,
            decoratorFactory);
    }
}

// source: https://github.com/khellang/Scrutor/blob/7f315dab5b0f7134a6be58941db1da8c904507e2/src/Scrutor/DecoratedService.cs
public sealed class DecoratedService<TService>
{
    internal DecoratedService(
        Type serviceType, IReadOnlyList<string> serviceKeys)
    {
        if (!typeof(TService).IsAssignableFrom(serviceType))
        {
            throw new ArgumentException($"The type {serviceType} is not assignable to the service type {typeof(TService)}");
        }

        ServiceType = serviceType;
        ServiceKeys = serviceKeys;
    }

    internal Type ServiceType { get; }
    internal IReadOnlyList<string> ServiceKeys { get; }
    internal DecoratedService<TService2> Downcast<TService2>()
        => new(ServiceType, ServiceKeys);
}

// source: https://github.com/khellang/Scrutor/blob/7f315dab5b0f7134a6be58941db1da8c904507e2/src/Scrutor/ClosedTypeDecorationStrategy.cs
internal sealed class ClosedTypeDecorationStrategy(
    Type serviceType,
    string? serviceKey,
    Type? decoratorType,
    Func<object, IServiceProvider, object>? decoratorFactory)
        : DecorationStrategy(serviceType, serviceKey)
{
    private Type? DecoratorType { get; } = decoratorType;
    private Func<object, IServiceProvider, object>? DecoratorFactory { get; } = decoratorFactory;

    protected override bool CanDecorate(
        Type serviceType)
        => ServiceType == serviceType;

    public override Func<IServiceProvider, object?, object> CreateDecorator(
        Type serviceType,
        string serviceKey) => this switch
        {
            { DecoratorType: not null }
                => TypeDecorator(serviceType, serviceKey, DecoratorType),
            { DecoratorFactory: not null }
                => FactoryDecorator(serviceType, serviceKey, DecoratorFactory),
            _
                => throw new InvalidOperationException(
                    message: $"Both {nameof(DecoratorType)} and {nameof(DecoratorFactory)} can not be null."),
        };
}