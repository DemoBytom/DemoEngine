using System;
using System.Threading.Tasks;
using Demo.Engine.Windows.Models.Options;
using Demo.Engine.Windows.Platform;
using Demo.Engine.Windows.Platform.Netstandard.Win32;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Windows
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
                    //TODO replace with serilog, move to extension "ConfigureSerilog" or somethings
                    .ConfigureLogging((hostingContext, configLog) =>
                    {
                        configLog.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        configLog.AddConsole();
                        configLog.AddDebug();
                    })
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
                return -1;
            }

            return 0;
        }
    }
}