// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using WorkItem = Demo.Engine.Core.Features.StaThread.StaThreadService.StaSingleThreadedSynchronizationContext.WorkItem;

namespace Demo.Engine.Core.Features.StaThread;

internal static class StaThreadRegistrationExtensions
{
    public static IServiceCollection AddStaThreadFeature(
        this IServiceCollection services)
    {
        _ = services
            .AddScoped<IStaThreadService, StaThreadService>()
            .AddScopedBoundedChannel<StaThreadRequests>(
                new BoundedChannelOptions(10)
                {
                    AllowSynchronousContinuations = false,
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = false,
                })
            .AddScoped<IStaThreadWriter, StaThreadWriter>()
            .AddStaWorkItemObjectPool()
            ;

        services
            .TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        services
            .TryAddSingleton(sp
                =>
            {
                var provider = sp.GetRequiredService<ObjectPoolProvider>();
                var policy = new DefaultPooledObjectPolicy<StaThreadRequests.DoEventsOkRequest>();

                var created = provider.Create(policy);

                return created;
            });

        return services;
    }

    internal static IServiceCollection AddStaWorkItemObjectPool(
        this IServiceCollection services)
    {
        services
            .TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

        services
            .TryAddSingleton(sp
                =>
            {
                var provider = sp.GetRequiredService<ObjectPoolProvider>();
                var policy = new DefaultPooledObjectPolicy<WorkItem>();
                var created = provider.Create(policy);
                return created;
            });

        return services;
    }
}