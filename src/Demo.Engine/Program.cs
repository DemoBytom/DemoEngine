using System;
using System.Threading.Tasks;
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
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Demo.Engine
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static async Task<int> Main(string[] args)
        {
            try
            {
                var hostBuilder = new HostBuilder()
                    .CreateDefault(args)
                    .WithSerilog()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddHostedService<EngineService>();
                        services.Configure<RenderSettings>(hostContext.Configuration.GetSection(nameof(RenderSettings)));
                        services.AddSingleton<IKeyboardCache, KeyboardCache>();
                        services.AddScoped<
                            ID3D11RenderingEngine,
                            IRenderingEngine,
                            D3D11RenderingEngine>();
                        services.AddScoped<IMainLoopService, MainLoopService>();
                        /*** Windows Only ***/
                        services.AddTransient<IRenderingControl, RenderingForm>();
                        services.AddScoped<IOSMessageHandler, WindowsMessagesHandler>();
                        services.AddTransient<IShaderCompiler, ShaderCompiler>();
                        //tmp
                        services.AddTransient<ICube, Cube>();
                        /*** End Windows Only ***/
                        services.AddMediatR(
                            typeof(KeyboardHandler).Assembly);
                    });

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
        }
    }
}