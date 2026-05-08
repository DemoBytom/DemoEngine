// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Engine.Core.Features.StaThread;

internal static class StaThreadRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddStaThreadFeature()
            => services
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

        private IServiceCollection AddSingletonBoundedChannel<T>(
            BoundedChannelOptions options)
            => services
                .AddSingleton(_
                    => Channel.CreateBounded<T>(
                        options))
                .AddSingleton(sp
                    => sp.GetRequiredService<Channel<T>>().Reader)
                .AddSingleton(sp
                    => sp.GetRequiredService<Channel<T>>().Writer)
            ;
    }
}