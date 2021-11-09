// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    public static class CoreHostExtensions
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
                _ = configHost
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables("DEMOENGINE_");

                if (args is object)
                {
                    _ = configHost.AddCommandLine(args);
                }
            })
            .ConfigureAppConfiguration((hostingContext, configApp) =>
            {
                var env = hostingContext.HostingEnvironment;
                _ = configApp
                    .AddJsonFile(appsettingsFile, optional: false, reloadOnChange: true);

                if (env.IsDevelopment())
                {
                    var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                    if (appAssembly is object)
                    {
                        _ = configApp.AddUserSecrets(appAssembly, optional: true);
                    }
                }

                if (args is object)
                {
                    _ = configApp.AddCommandLine(args);
                }
            })
            .ConfigureServices(services =>
                //supresses the default "Application started. Press Ctrl+C to shut down." etc. log messages, that ConsoleLifetime produces
                services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true))
            .UseDefaultServiceProvider((context, options) =>
            {
                var isDev = context.HostingEnvironment.IsDevelopment();
                options.ValidateOnBuild = isDev;
                options.ValidateScopes = isDev;
            });
    }
}