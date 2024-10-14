// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Demo.Engine.Platform.DirectX.Bindable.Buffers;

public class IndexBuffer<T> : Buffer<T>, IIndexBuffer
    where T : unmanaged
{
    private readonly Format _format;
    public uint IndexCount { get; }

    public IndexBuffer(
        ID3D11RenderingEngine renderingEngine,
        in T[] data,
        uint sizeInBytes,
        Format format = Format.R16_UInt)
        : base(
            renderingEngine,
            data,
            new BufferDescription
            {
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.IndexBuffer,
                MiscFlags = ResourceOptionFlags.None,
                CPUAccessFlags = CpuAccessFlags.None,
                StructureByteStride = sizeInBytes,
                ByteWidth = (uint)data.Length * sizeInBytes,
            })
    {
        _format = format;
        IndexCount = (uint)data.Length;
    }

    public override void Bind() => _renderingEngine.DeviceContext
        .IASetIndexBuffer(_buffer, _format, 0);
}