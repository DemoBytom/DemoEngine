// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Core.Interfaces;

internal interface IMainLoopServiceNew
{
    Task ExecutingTask { get; }
}