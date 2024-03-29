// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Platform.DirectX.Interfaces;

namespace Demo.Engine.Platform.DirectX.Bindable.Buffers;

/// <summary>
/// Pixel shader constant buffer that can be bound to the rendering pipeline
/// </summary>
/// <typeparam name="T"></typeparam>
public class PSConstantBuffer<T> : ConstantBuffer<T>
    where T : unmanaged
{
    public PSConstantBuffer(
        ID3D11RenderingEngine renderingEngine,
        ref T data)
        : base(
            renderingEngine,
            ref data)
    {
    }

    public override void Bind() => _renderingEngine.DeviceContext
        .PSSetConstantBuffer(0, _buffer);
}