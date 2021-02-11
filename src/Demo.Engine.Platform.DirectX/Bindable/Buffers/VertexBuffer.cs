using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Bindable.Buffers
{
    public class VertexBuffer<T> : Buffer<T>
        where T : unmanaged
    {
        private readonly VertexBufferView _vertexBufferView;

        public VertexBuffer(
            ID3D11RenderingEngine renderingEngine,
            T[] data,
            int sizeInBytes)
            : base(
                renderingEngine,
                data,
                new BufferDescription
                {
                    Usage = Usage.Default,
                    BindFlags = BindFlags.VertexBuffer,
                    OptionFlags = ResourceOptionFlags.None,
                    CpuAccessFlags = CpuAccessFlags.None,
                    StructureByteStride = sizeInBytes,
                    SizeInBytes = data.Length * sizeInBytes,
                }) => _vertexBufferView = new VertexBufferView(_buffer, sizeInBytes);

        public override void Bind() => _renderingEngine.DeviceContext
            .IASetVertexBuffers(0, _vertexBufferView);
    }
}