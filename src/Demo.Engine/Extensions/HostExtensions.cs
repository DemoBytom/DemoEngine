// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Serilog;

namespace Microsoft.Extensions.Hosting
{
    public static class HostExtensions
    {
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
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Debug()
                .CreateLogger();
            return hostBuilder
                .ConfigureLogging((_, configLog) =>
                    //configLog.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    configLog.AddSerilog(Log.Logger));
        }
    }
}