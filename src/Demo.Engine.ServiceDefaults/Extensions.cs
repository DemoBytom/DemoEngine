// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Observability.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace Demo.Engine.ServiceDefaults;

public static class Extensions
{
    extension<TBuilder>(TBuilder builder)
        where TBuilder : IHostBuilder
    {
        public TBuilder AddServiceDefaults(
            TelemetryBuilderFunc<TelemetryBuilder> instrumentations)
            => builder
                .ConfigureOpenTelemetry(
                    instrumentations)
                ;

        private TBuilder ConfigureOpenTelemetry<TTelemetryBuilder>(
            TelemetryBuilderFunc<TTelemetryBuilder>? instrumentations = null)
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
                    .RegisterInstrumentation(instrumentations)
                    .WithMetrics(metrics => metrics
                        .AddRuntimeInstrumentation())
                    .WithLogging())
                .AddOpenTelemetryExporters()
                ;

        private TBuilder AddOpenTelemetryExporters()
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

    public delegate ref TTelemetryBuilder TelemetryBuilderFunc<TTelemetryBuilder>(
        ref TTelemetryBuilder builder)
        where TTelemetryBuilder : ITelemetryBuilder<TTelemetryBuilder>, allows ref struct;

    extension<TInnerBuilder>(TInnerBuilder innerBuilder)
    {
        private TInnerBuilder RegisterInstrumentation<TTelemetryBuilder>(
            TelemetryBuilderFunc<TTelemetryBuilder>? instrumentations = null)
            where TTelemetryBuilder : ITelemetryBuilder<TTelemetryBuilder, TInnerBuilder>, allows ref struct
        {
            if (instrumentations is not null)
            {
                var telemetryBuilder = TTelemetryBuilder.Create(innerBuilder);
                return instrumentations(ref telemetryBuilder).Builder;
            }
            return innerBuilder;
        }
    }
}