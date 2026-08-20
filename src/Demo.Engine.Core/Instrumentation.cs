// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Observability.Abstractions;

namespace Demo.Engine.Core;

[Instrumentation<Instrumentation>(
    name: "Demo.Engine",
    sourceName: "Demo.Engine")]
public sealed partial class Instrumentation
    : IInstrumentation
{
}