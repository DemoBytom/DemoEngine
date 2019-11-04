using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Microsoft.Extensions.Hosting
{
    public static class HostExtensions
    {
        /// <summary>
        /// Creates a default (yet different than <see cref="Host.CreateDefaultBuilder(string[])"/>)
        /// host builder
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="args">command line arguments</param>
        /// <param name="appsettingsFile">appsettings filename, default is "appsettings.json"</param>
        /// <returns>Preconfigured <see cref="IHostBuilder"/></returns>
        public static IHostBuilder CreateDefault(
            this IHostBuilder hostBuilder,
            string[] args,
            string appsettingsFile = "appsettings.json") =>
        hostBuilder
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureHostConfiguration((configHost) =>
            {
                configHost.SetBasePath(Directory.GetCurrentDirectory());
                if (args != null)
                {
                    configHost.AddCommandLine(args);
                }
            })
            .ConfigureAppConfiguration((hostingContext, configApp) =>
            {
                var env = hostingContext.HostingEnvironment;
                configApp.AddJsonFile(appsettingsFile, optional: false, reloadOnChange: true);

                if (Debugger.IsAttached)
                {
                    var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                    if (appAssembly != null)
                    {
                        configApp.AddUserSecrets(appAssembly, optional: true);
                    }
                }

                if (args != null)
                {
                    configApp.AddCommandLine(args);
                }
            })
            .ConfigureServices(services =>
            {
                //supresses the default "Application started. Press Ctrl+C to shut down." etc. log messages, that ConsoleLifetime produces
                services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
            })
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateOnBuild = true;
                options.ValidateScopes = true;
            });

        /// <summary>
        /// Creates a default logger that is assigned to <see cref="Log.Logger"/> as well as
        /// registers it in the logging pipeline.
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IHostBuilder WithSerilog(
            this IHostBuilder hostBuilder)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Debug()
                .CreateLogger();
            return hostBuilder
                .ConfigureLogging((hostingContext, configLog) =>
                {
                    //configLog.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    configLog.AddSerilog(Log.Logger);
                });
        }
    }
}