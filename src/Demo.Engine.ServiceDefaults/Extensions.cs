// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Observability.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using static Demo.Engine.Observability.Abstractions.InstrumentationExtensions;

namespace Demo.Engine.ServiceDefaults;

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

    private static TBuilder ConfigureOpenTelemetry<TBuilder>(
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

    public static NewTelemetryBuilder WithInstrumentation<TInstrumentation>(
        this OpenTelemetryBuilder builder)
        => new(builder);

    extension<TBuilder>(TBuilder builder)
        where TBuilder : IHostBuilder
    {
        public TBuilder AddServiceDefaults2(
            TelemetryBuilderFunc<NewTelemetryBuilder, OpenTelemetryBuilder> instrumentations)
        {

            return builder
                .ConfigureOpenTelemetry2(
                    instrumentations)
                ;
        }

        private TBuilder ConfigureOpenTelemetry2<TTelemetryBuilder>(
            TelemetryBuilderFunc<TTelemetryBuilder, OpenTelemetryBuilder>? instrumentations = null)
            where TTelemetryBuilder : ITelemetryBuilder<TTelemetryBuilder, OpenTelemetryBuilder>, allows ref struct
            => (TBuilder)builder
                .ConfigureLogging(logging => logging
                    .AddOpenTelemetry(options =>
                    {
                        options.IncludeFormattedMessage = true;
                        options.IncludeScopes = true;
                    }))
                .ConfigureServices((hostContext, services) => services
                    .AddOpenTelemetry()
                    .RegisterInstrumentation2(instrumentations)
                    .WithMetrics(metrics => metrics
                        .AddRuntimeInstrumentation())
                    .WithLogging())
                .AddOpenTelemetryExporters()
                ;
    }

    extension<TInnerBuilder>(TInnerBuilder innerBuilder)
    {
        public TInnerBuilder RegisterInstrumentation2<TTelemetryBuilder>(
            TelemetryBuilderFunc<TTelemetryBuilder, TInnerBuilder>? instrumentations = null)
            where TTelemetryBuilder : ITelemetryBuilder<TTelemetryBuilder, TInnerBuilder>, allows ref struct
        {
            if (instrumentations is not null)
            {
                var telemetryBuilder = TTelemetryBuilder.Create(innerBuilder);
                return instrumentations(in telemetryBuilder).Builder;
            }
            return innerBuilder;
        }
    }

    public static ref readonly NewTelemetryBuilder WithInstrumentation<TInstrumentation>(
        ref readonly this NewTelemetryBuilder builder)
        where TInstrumentation : IInstrumentation
    {
        builder.Build<TInstrumentation>();
        return ref builder;
    }
}

public readonly ref struct NewTelemetryBuilder
    : ITelemetryBuilder<NewTelemetryBuilder, OpenTelemetryBuilder>
{
    public NewTelemetryBuilder()
        => throw new NotSupportedException(
            "Parameterless construction is not supported!");

    internal NewTelemetryBuilder(
        OpenTelemetryBuilder builder)
        => Builder = builder;

    public OpenTelemetryBuilder Builder { get; }

    static NewTelemetryBuilder ITelemetryBuilder<NewTelemetryBuilder, OpenTelemetryBuilder>.Create(
        OpenTelemetryBuilder builder)
        => new NewTelemetryBuilder(builder);

    public void Build<TInstrumentation>()
        where TInstrumentation : IInstrumentation
    {
        Builder
            .WithMetrics(metrics => metrics
                .AddMeter(TInstrumentation.INSTRUMENTATION_SOURCE_NAME))
            .WithTracing(tracing => tracing
                .AddSource(TInstrumentation.INSTRUMENTATION_SOURCE_NAME))
            ;
    }
}