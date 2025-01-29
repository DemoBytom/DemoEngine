// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Components.Keyboard.Internal;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Platform;
using Demo.Engine.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Engine.Core;

public static class RegistrationExtensions
{
    public static IServiceCollection AddEngineCore(
        this IServiceCollection services)
        => services
            .AddScoped<IMainLoopService, MainLoopService>()
            .AddSingleton<IKeyboardCache, KeyboardCache>()
            .AddTransient<IFpsTimer, FpsTimer>()
            .AddTransient<IContentFileProvider, ContentFileProvider>()
            .AddHostedService<EngineServiceNew>()
        ;
}