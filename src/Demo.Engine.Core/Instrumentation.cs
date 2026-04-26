// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace Demo.Engine.Core;

public static class Instrumentation
{
    public const string APPLICATION_NAME = "Demo.Engine";

    public static readonly string VERSION = Assembly
        .GetExecutingAssembly()?
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion
        ?? "0.0.0";

    public static readonly Meter Meter = new(
        name: APPLICATION_NAME,
        version: VERSION);

    public static readonly ActivitySource ActivitySource = new(
        name: APPLICATION_NAME,
        version: VERSION);
}