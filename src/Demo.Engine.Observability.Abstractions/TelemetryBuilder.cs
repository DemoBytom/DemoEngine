// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using OpenTelemetry;

namespace Demo.Engine.Observability.Abstractions;

public readonly ref struct TelemetryBuilder
{
    public TelemetryBuilder()
        => throw new NotSupportedException(
            "Parameterless construction is not supported!");

    internal TelemetryBuilder(
        OpenTelemetryBuilder builder)
        => Builder = builder;

    internal OpenTelemetryBuilder Builder { get; }
}

public interface ITelemetryBuilder<TTelemetryBuilder, TInnerBuilder>
    where TTelemetryBuilder : ITelemetryBuilder<TTelemetryBuilder, TInnerBuilder>, allows ref struct
{
    abstract void Build<TInstrumentation>()
        where TInstrumentation : IInstrumentation
        ;

    abstract static TTelemetryBuilder Create(TInnerBuilder builder);

    public TInnerBuilder Builder { get; }
}