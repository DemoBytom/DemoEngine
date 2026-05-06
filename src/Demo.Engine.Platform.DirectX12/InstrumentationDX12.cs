// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using Demo.Engine.Observability.Abstractions;

namespace Demo.Engine.Platform.DirectX12;

internal sealed class InstrumentationDX12
    : IInstrumentation
{
    public static string INSTRUMENTATION_SOURCE_NAME => "Demo.Engine.Platform.DirectX12";

    public static string VERSION => typeof(InstrumentationDX12)
        .Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion
        ?? "0.0.0";

    public static Meter Meter { get; } = new Meter(
        name: INSTRUMENTATION_SOURCE_NAME,
        version: VERSION);

    public static ActivitySource ActivitySource { get; } = new ActivitySource(
        name: INSTRUMENTATION_SOURCE_NAME,
        version: VERSION);
}