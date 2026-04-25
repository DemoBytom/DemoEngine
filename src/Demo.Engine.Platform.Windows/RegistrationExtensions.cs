// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Windows.Platform.Netstandard.Win32;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Engine.Platform.Windows;

public static class RegistrationExtensions
{
    public static IServiceCollection AddPlatformWindows(
        this IServiceCollection services)
        => services
            .AddHostedService<WindowsMessagePump>()
            .AddScoped<IRenderingControl, RenderingForm>()
        ;

}