// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Platform.DirectX12.Shaders;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Engine.Platform.DirectX12;

public static class RegistrationExtensions
{
    public static IServiceCollection AddDirectX12(
        this IServiceCollection services)
        => services
            .AddScoped<D3D12RenderingEngine>()
            .AddScoped<IRenderingEngine>(sp
                => sp.GetRequiredService<D3D12RenderingEngine>())
            .AddScoped<ID3D12RenderingEngine>(sp
                => sp.GetRequiredService<D3D12RenderingEngine>())
            .AddScoped<IDebugLayerLogger, DebugLayerLogger>()
            // shaders
            .AddSingleton<IShaderCompiler, ShaderCompilerOld>()
            .AddSingleton<IShaderAsyncCompiler, ShaderCompiler>()
            .AddScoped<IEngineShaderManager, EngineShaderManager>()
            .AddScoped<EngineShaderManager>()
            //Cube
            .AddScoped<ICube, Cube>()
        ;
}