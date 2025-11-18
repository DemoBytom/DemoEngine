// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Core.Interfaces;

internal interface IMainLoopLifetime
{
    /// <inheritdoc cref="CancellationTokenSource.Cancel()"/>
    CancellationToken Token { get; }

    /// <inheritdoc cref="CancellationTokenSource.Token"/>
    void Cancel();
}