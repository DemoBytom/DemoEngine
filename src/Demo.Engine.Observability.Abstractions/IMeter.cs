// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.Metrics;

namespace Demo.Engine.Observability.Abstractions;

public interface IMeter
{
    static abstract Meter Meter { get; }
}