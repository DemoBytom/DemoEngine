// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Observability.Abstractions;

public interface ITelemetryBuilder<TTelemetryBuilder>
    where TTelemetryBuilder : ITelemetryBuilder<TTelemetryBuilder>, allows ref struct
{
    abstract TTelemetryBuilder WithInstrumentation<TInstrumentation>()
        where TInstrumentation : IInstrumentation;
}

public interface ITelemetryBuilder<TTelemetryBuilder, TInnerBuilder>
    : ITelemetryBuilder<TTelemetryBuilder>
    where TTelemetryBuilder : ITelemetryBuilder<TTelemetryBuilder, TInnerBuilder>, allows ref struct
{
    static abstract TTelemetryBuilder Create(TInnerBuilder builder);

    TInnerBuilder Builder { get; }
}