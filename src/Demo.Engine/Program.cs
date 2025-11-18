// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Autofac.Extensions.DependencyInjection;
using Demo.Engine.Core;
using Demo.Engine.Core.Components.Keyboard.Internal;
using Demo.Engine.Core.Extensions;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Models.Options;
using Demo.Engine.Extensions;
using Demo.Engine.Platform.DirectX12;
using Demo.Engine.Platform.Windows;
using Demo.Engine.Windows.Platform.Netstandard.Win32;
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
            .Configure<RenderSettings>(hostContext.Configuration.GetSection(nameof(RenderSettings)))
            /*** Windows Only ***/
            .AddScoped<IRenderingControl, RenderingForm>()
            .AddScoped<IOSMessageHandler, WindowsMessagesHandler>()
            .AddEngineCore()
            .AddDirectX12()
            /*** End Windows Only ***/
            .AddMediatR(config
                => config.RegisterServicesFromAssemblyContaining<KeyboardHandler>());

            _ = services.AddOptions();

            _ = services
                .AddSingleton(x =>
                    new Demo.Engine.Platform.DirectX12.Shaders.CompiledVS("Shaders/Triangle/TriangleVS.hlsl", x.GetRequiredService<IShaderCompiler>()))
                .AddSingleton(x =>
                    new Demo.Engine.Platform.DirectX12.Shaders.CompiledPS("Shaders/Triangle/TrianglePS.hlsl", x.GetRequiredService<IShaderCompiler>()));
        })
        ;

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