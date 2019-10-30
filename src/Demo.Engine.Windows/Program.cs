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
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();
                    services.AddHostedService<EngineService>();
                    services.Configure<FormSettings>(formSettings =>
                    {
                        formSettings.Width = 1024;
                        formSettings.Height = 768;
                    });
                    services.AddTransient<IRenderingFormFactory, RenderingFormFactory>();
                })
                .Build();
            //rf.Show();
            await host.RunAsync();

            return 0;
        }
    }
}