// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Core.Interfaces.Platform;

internal interface IStaThreadService
{
    Task ExecutingTask { get; }
    bool IsRunning { get; }

    void Cancel();
}