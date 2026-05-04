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