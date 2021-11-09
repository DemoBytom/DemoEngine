// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.DirectX.Interfaces;

internal interface IUpdatable : IBindable
{
}

internal interface IUpdatable<T> : IUpdatable
{
    public void Update(in T data);
}