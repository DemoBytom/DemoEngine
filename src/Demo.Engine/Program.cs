using System;
using System.Threading.Tasks;
using Demo.Engine.Core.Models.Options;
using Demo.Engine.Core.Platform;
using Demo.Engine.Core.Services;
using Demo.Engine.Platform.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Demo.Engine
{
    internal class Program
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
                        services.Configure<FormSettings>(hostContext.Configuration.GetSection(nameof(FormSettings)));
                        services.AddTransient<IRenderingFormFactory, RenderingFormFactory>();
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
