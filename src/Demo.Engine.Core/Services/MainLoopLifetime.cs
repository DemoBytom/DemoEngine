// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces;

namespace Demo.Engine.Core.Services;

internal sealed class MainLoopLifetime
    : IMainLoopLifetime,
      IDisposable
{
    private readonly CancellationTokenSource _loopCancellationTokenSource = new();
    private bool _disposedValue;

    /// <inheritdoc cref="CancellationTokenSource.Cancel()"/>
    public void Cancel()
        => _loopCancellationTokenSource.Cancel();

    /// <inheritdoc cref="CancellationTokenSource.Token"/>
    public CancellationToken Token
        => _loopCancellationTokenSource.Token;

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _loopCancellationTokenSource.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}