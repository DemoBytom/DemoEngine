// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Tools.Common.Interfaces;

public interface IAsyncInitializable<TService>
{
    ValueTask<TService> InitializeAsync(CancellationToken cancellationToken = default);
}