// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Demo.Engine.Core.Components.Keyboard.Internal;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Models.Options;
using Demo.Engine.Core.Services;
using Demo.Engine.Platform.DirectX;
using Demo.Engine.Platform.DirectX.Interfaces;
using Demo.Engine.Platform.DirectX.Models;
using Demo.Engine.Platform.DirectX.Shaders;
using Demo.Engine.Platform.Windows;
using Demo.Engine.Windows.Platform.Netstandard.Win32;
using Demo.Tools.Common.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

try
{
    var hostBuilder = new HostBuilder()
        .CreateDefault(args)
        .WithSerilog()
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureServices((hostContext, services)
        =>
        {
            _ = services
            .AddHostedService<EngineService>()
            .Configure<RenderSettings>(hostContext.Configuration.GetSection(nameof(RenderSettings)))
            .AddSingleton<IKeyboardCache, KeyboardCache>()
            .AddScoped<
                ID3D11RenderingEngine,
                IRenderingEngine,
                D3D11RenderingEngine>()
            .AddScoped<IMainLoopService, MainLoopService>()
            /*** Windows Only ***/
            .AddTransient<IRenderingControl, RenderingForm>()
            .AddScoped<IOSMessageHandler, WindowsMessagesHandler>()
            .AddTransient<IShaderCompiler, ShaderCompiler>()
            .AddScoped<IDebugLayerLogger, DebugLayerLogger>()
            //tmp
            //.AddTransient<ICube, Cube>()
            /*** End Windows Only ***/
            .AddMediatR(config
                => config.RegisterServicesFromAssemblyContaining<KeyboardHandler>());

            _ = services.AddOptions();

            _ = services
                .AddSingleton(x =>
                    new CompiledVS("Shaders/Triangle/TriangleVS.hlsl", x.GetRequiredService<IShaderCompiler>()))
                .AddSingleton(x =>
                    new CompiledPS("Shaders/Triangle/TrianglePS.hlsl", x.GetRequiredService<IShaderCompiler>()));
        })
        .ConfigureContainer<ContainerBuilder>(builder
            => builder
                .RegisterType<Cube>()
                .As<ICube>()
                .ExternallyOwned());

    var host = hostBuilder.Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    //log fatal
    Log.Fatal(ex, "FATAL ERROR!");
    return -1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;