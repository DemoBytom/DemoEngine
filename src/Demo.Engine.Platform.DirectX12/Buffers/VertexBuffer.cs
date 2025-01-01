// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Vortice.Direct3D12;

namespace Demo.Engine.Platform.DirectX12.Buffers;

internal class VertexBuffer<TVertices>
    : IDisposable
    where TVertices : unmanaged, ISizeInBytes<TVertices>
{
    private readonly ID3D12RenderingEngine _renderingEngine;
    private readonly ID3D12Resource _vertexBuffer;
    private readonly ID3D12Resource _vertexUploadBuffer;
    private readonly VertexBufferView _vertexBufferView;
    private bool _disposedValue;

    public VertexBuffer(
        ID3D12RenderingEngine renderingEngine,
        ReadOnlySpan<TVertices> vertices)
    {
        _renderingEngine = renderingEngine;

        var bufferDescription = ResourceDescription.Buffer(
            sizeInBytes: TVertices.SizeInBytes * (ulong)vertices.Length,
            flags: ResourceFlags.None,
            alignment: D3D12.DefaultResourcePlacementAlignment);

        _vertexBuffer = _renderingEngine.Device.CreateCommittedResource(
            heapProperties: HeapProperties.DefaultHeapProperties,
            heapFlags: HeapFlags.None,
            description: bufferDescription,
            initialResourceState: ResourceStates.Common,
            optimizedClearValue: null);

        _vertexUploadBuffer = _renderingEngine.Device.CreateCommittedResource(
            heapProperties: HeapProperties.UploadHeapProperties,
            heapFlags: HeapFlags.None,
            description: ResourceDescription.Buffer(
                TVertices.SizeInBytes * (ulong)vertices.Length,
                ResourceFlags.None,
                D3D12.DefaultResourcePlacementAlignment),
            initialResourceState: ResourceStates.Common,
            optimizedClearValue: null);

        var cpuBufferData = _vertexUploadBuffer.Map<TVertices>(
            0,
            vertices.Length);

        vertices.CopyTo(cpuBufferData);

        _vertexUploadBuffer.Unmap(0);

        _renderingEngine.InitCommandList();

        _renderingEngine.CommandList
           .CopyBufferRegion(
               dstBuffer: _vertexBuffer,
               dstOffset: 0,
               srcBuffer: _vertexUploadBuffer,
               srcOffset: 0,
               numBytes: Vertex.SizeInBytes * (ulong)vertices.Length);

        _renderingEngine.ExecuteCommandList();

        _vertexBufferView = new VertexBufferView(
            _vertexBuffer.GPUVirtualAddress,
            sizeInBytes: TVertices.SizeInBytes * (uint)vertices.Length,
            strideInBytes: TVertices.SizeInBytes);
    }

    public void Bind()
        => _renderingEngine.CommandList.IASetVertexBuffers(
            slot: 0,
            _vertexBufferView);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _vertexBuffer.Dispose();
                _vertexUploadBuffer.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}