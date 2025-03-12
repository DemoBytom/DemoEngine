// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;

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
}