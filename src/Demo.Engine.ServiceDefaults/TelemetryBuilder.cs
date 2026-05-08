// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Observability.Abstractions;
using OpenTelemetry;

namespace Demo.Engine.ServiceDefaults;

public readonly ref struct TelemetryBuilder
    : ITelemetryBuilder<TelemetryBuilder, OpenTelemetryBuilder>
{
    public TelemetryBuilder()
        => throw new NotSupportedException(
            "Parameterless construction is not supported!");

    internal TelemetryBuilder(
        OpenTelemetryBuilder builder)
        => Builder = builder;

    public OpenTelemetryBuilder Builder { get; }

    static TelemetryBuilder ITelemetryBuilder<TelemetryBuilder, OpenTelemetryBuilder>.Create(
        OpenTelemetryBuilder builder)
        => new(builder);

    public void WithInstrumentation<TInstrumentation>()
        where TInstrumentation : IInstrumentation
        => _ = Builder
            .WithMetrics(metrics => metrics
                .AddMeter(TInstrumentation.INSTRUMENTATION_SOURCE_NAME))
            .WithTracing(tracing => tracing
                .AddSource(TInstrumentation.INSTRUMENTATION_SOURCE_NAME))
            ;
}