// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Microsoft.Extensions.Hosting;
using Serilog;

namespace Demo.Engine.Extensions;

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
            .WriteTo.Async(writeTo =>
                writeTo.Console()
                .WriteTo.Debug())
            //.WriteTo.Console()
            //.WriteTo.Debug()
            .CreateLogger();
        return hostBuilder
            .ConfigureLogging((_, configLog) =>
                //configLog.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                configLog.AddSerilog(Log.Logger));
    }
}