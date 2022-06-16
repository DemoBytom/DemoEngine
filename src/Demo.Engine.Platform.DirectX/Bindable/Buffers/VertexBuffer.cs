// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System;
using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Bindable.Buffers;

public class VertexBuffer<T> : Buffer<T>
    where T : unmanaged
{
    private readonly int _sizeInBytes;

    public VertexBuffer(
        ID3D11RenderingEngine renderingEngine,
        ReadOnlySpan<T> data,
        int sizeInBytes)
        : base(
            renderingEngine,
            data,
            new BufferDescription
            {
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.VertexBuffer,
                MiscFlags = ResourceOptionFlags.None,
                CPUAccessFlags = CpuAccessFlags.None,
                StructureByteStride = sizeInBytes,
                ByteWidth = data.Length * sizeInBytes,
            })
        => _sizeInBytes = sizeInBytes;

    public override void Bind() => _renderingEngine.DeviceContext
        .IASetVertexBuffer(0, _buffer, _sizeInBytes);
}