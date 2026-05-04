// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using Demo.Engine.Observability.Abstractions;

namespace Demo.Engine.Core;

public sealed class Instrumentation
    : IInstrumentation
{
    public static string INSTRUMENTATION_SOURCE_NAME => "Demo.Engine";

    public static string VERSION { get; } = Assembly
        .GetExecutingAssembly()?
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion
        ?? "0.0.0";

    public static Meter Meter { get; } = new(
        name: INSTRUMENTATION_SOURCE_NAME,
        version: VERSION);

    public static ActivitySource ActivitySource { get; } = new(
        name: INSTRUMENTATION_SOURCE_NAME,
        version: VERSION);
}