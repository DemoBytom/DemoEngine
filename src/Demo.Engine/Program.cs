// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core;
using Demo.Engine.Core.Extensions;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Models.Options;
using Demo.Engine.Extensions;
using Demo.Engine.Platform.DirectX12;
using Demo.Engine.Platform.Windows;
using Demo.Engine.ServiceDefaults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

try
{
    var hostBuilder = new HostBuilder()
        .CreateDefault(args)
        .AddServiceDefaults(
            (ref readonly instrumentation) => ref instrumentation
                .WithInstrumentation<Instrumentation>())
        .WithSerilog()
        .ConfigureServices((hostContext, services)
        =>
        {
            _ = services
            .Configure<RenderSettings>(hostContext.Configuration.GetSection(nameof(RenderSettings)))
            /*** Windows Only ***/
            .AddEngineCore()
            .AddPlatformWindows()
            .AddDirectX12()
            /*** End Windows Only ***/
            .AddMediator(options =>
            {
                options.Namespace = "Demo.Engine";
                options.GenerateTypesAsInternal = true;
            })
            ;

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