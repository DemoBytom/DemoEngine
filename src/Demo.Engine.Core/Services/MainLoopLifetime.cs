// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces;

namespace Demo.Engine.Core.Services;

internal sealed class MainLoopLifetime
    : IMainLoopLifetime
{
    private readonly CancellationTokenSource _loopCancellationTokenSource = new();

    /// <inheritdoc cref="CancellationTokenSource.Cancel()"/>
    public void Cancel()
        => _loopCancellationTokenSource.Cancel();

    /// <inheritdoc cref="CancellationTokenSource.Token"/>
    public CancellationToken Token
        => _loopCancellationTokenSource.Token;
}