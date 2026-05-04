// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

namespace Demo.Engine.Observability.Abstractions;

public static class InstrumentationExtensions
{
    public static ref readonly TelemetryBuilder WithMetricsAndTracing<TInstrumentation>(
        ref readonly this TelemetryBuilder builder)
        where TInstrumentation : IInstrumentation
    {
        _ = builder.Builder
            .WithMetrics(metrics => metrics
                .AddMeter(TInstrumentation.INSTRUMENTATION_SOURCE_NAME))
            .WithTracing(tracing => tracing
                .AddSource(TInstrumentation.INSTRUMENTATION_SOURCE_NAME))
            ;

        return ref builder;
    }

    public static TBuilder AddInstrumentations<TBuilder>(
        this TBuilder builder,
        TelemetryBuilderFunc? instrumentations = null)
        where TBuilder : IHostBuilder
    => (TBuilder)builder
        .ConfigureServices((hostContext, services) => services
            .AddOpenTelemetry()
            .RegisterInstrumentation(instrumentations))
        ;

    internal static OpenTelemetryBuilder RegisterInstrumentation(
        this OpenTelemetryBuilder builder,
        TelemetryBuilderFunc? instrumentations = null)
    {
        if (instrumentations is not null)
        {
            var telemetryBuilder = new TelemetryBuilder(builder);
            return instrumentations(in telemetryBuilder).Builder;
        }

        return builder;
    }

    public delegate ref readonly TelemetryBuilder TelemetryBuilderFunc(
        ref readonly TelemetryBuilder builder);
}