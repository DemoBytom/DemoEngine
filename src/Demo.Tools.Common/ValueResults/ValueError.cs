// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Tools.Common.ValueResults;

public readonly ref struct ValueError(
    string error)
    : IError
{
    public string Message { get; } = error;
}