// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Observability.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using static Demo.Engine.Observability.Abstractions.InstrumentationExtensions;

namespace DemoEngine.ServiceDefaults;

public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(
        this TBuilder builder,
        TelemetryBuilderFunc? instrumentations = null)
        where TBuilder : IHostBuilder
        => builder
            .ConfigureOpenTelemetry(
                instrumentations)
        ;
    public static ref readonly TelemetryBuilder WithInstrumentation<TInstrumentation>(
        ref readonly this TelemetryBuilder builder)
        where TInstrumentation : IInstrumentation
        => ref InstrumentationExtensions.WithMetricsAndTracing<TInstrumentation>(
            in builder);

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(
        this TBuilder builder,
        TelemetryBuilderFunc? instrumentations = null)
        where TBuilder : IHostBuilder
        => (TBuilder)builder
            .ConfigureLogging(logging => logging
                .AddOpenTelemetry(options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                }))
            .ConfigureServices((hostContext, services) => services
                .AddOpenTelemetry()
                .WithMetrics(metrics => metrics
                    .AddRuntimeInstrumentation())
                .WithLogging())
            .AddInstrumentations(instrumentations)
            .AddOpenTelemetryExporters()
            ;

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(
        this TBuilder builder)
        where TBuilder : IHostBuilder
        => (TBuilder)builder.ConfigureServices((hostContext, services) =>
        {
            var useOtlpExporter = !string.IsNullOrWhiteSpace(
                hostContext.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
            if (useOtlpExporter)
            {
                _ = services
                    .AddOpenTelemetry()
                    .UseOtlpExporter()
                    ;
            }
        });
}