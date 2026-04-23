// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Core.Features.StaThread;

public interface IStaThreadReader
{
    Task Invoke(
        Func<Func<CancellationToken, ValueTask<bool>>, CancellationToken, Task<bool>> callback,
        CancellationToken cancellationToken = default);
}