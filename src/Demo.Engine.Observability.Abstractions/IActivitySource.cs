// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;

namespace Demo.Engine.Observability.Abstractions;

public interface IActivitySource
{
    static abstract ActivitySource ActivitySource { get; }
}