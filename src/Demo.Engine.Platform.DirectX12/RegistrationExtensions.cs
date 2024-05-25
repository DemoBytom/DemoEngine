// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Engine.Platform.DirectX12;

public static class RegistrationExtensions
{
    public static IServiceCollection AddDirectX12(
        this IServiceCollection services)
        => services
            .AddSingleton<IRenderingEngine, D3D12RenderingEngine>()
            .AddSingleton<IDebugLayerLogger, DebugLayerLogger>()
        ;
}