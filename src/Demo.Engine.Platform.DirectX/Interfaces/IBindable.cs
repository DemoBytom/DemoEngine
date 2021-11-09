// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.DirectX.Interfaces;

/// <summary>
/// An object that can be bound to the rendering pipeline
/// </summary>
internal interface IBindable
{
    /// <summary>
    /// Method used to bind to the rendering pipeline
    /// </summary>
    void Bind();
}