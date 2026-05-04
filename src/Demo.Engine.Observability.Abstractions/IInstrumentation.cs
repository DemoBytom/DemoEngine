// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Observability.Abstractions;

public interface IInstrumentation
    : IMeter
    , IActivitySource
{
    static abstract string INSTRUMENTATION_SOURCE_NAME { get; }
    static abstract string VERSION { get; }
}