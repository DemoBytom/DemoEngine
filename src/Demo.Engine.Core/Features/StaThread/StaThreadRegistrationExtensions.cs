// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Engine.Core.Features.StaThread;

internal static class StaThreadRegistrationExtensions
{
    public static IServiceCollection AddStaThreadFeature(
        this IServiceCollection services)
    {
        _ = services
            .AddSingletonBoundedChannel<StaThreadRequests>(
                new BoundedChannelOptions(10)
                {
                    AllowSynchronousContinuations = false,
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = false,
                })
            .AddSingleton<IStaThreadWriter, StaThreadWriter>()
            .AddSingleton<IStaThreadReader, StaThreadReader>()
            ;

        return services;
    }
}