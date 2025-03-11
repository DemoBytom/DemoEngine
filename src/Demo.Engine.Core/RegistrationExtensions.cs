// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Threading.Channels;
using Demo.Engine.Core.Components.Keyboard.Internal;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Platform;
using Demo.Engine.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;

namespace Demo.Engine.Core;

public static class RegistrationExtensions
{
    public static IServiceCollection AddEngineCore(
        this IServiceCollection services)
    {
        _ = services
                .AddScoped<IMainLoopService, MainLoopService>()
                .AddSingleton<IKeyboardCache, KeyboardCache>()
                .AddTransient<IFpsTimer, FpsTimer>()
                .AddTransient<IContentFileProvider, ContentFileProvider>()
                .AddHostedService<EngineServiceNew>()
                .AddScoped<IMainLoopLifetime, MainLoopLifetime>()
                .AddScoped<IStaThreadService, StaThreadService>()
                .AddScoped<ILoopJob, LoopJob>()
                .AddScoped<IMainLoopServiceNew, MainLoopServiceNew>()
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

    private static IServiceCollection AddScopedBoundedChannel<T>(
        this IServiceCollection services,
            BoundedChannelOptions options)
        => services
            .AddScoped(_
                => Channel.CreateBounded<T>(
                    options))
            .AddScoped(sp
                => sp.GetRequiredService<Channel<T>>().Reader)
            .AddScoped(sp
                => sp.GetRequiredService<Channel<T>>().Writer)
        ;
}