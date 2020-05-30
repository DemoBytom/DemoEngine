using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Demo.Engine.Platform.DirectX.Bindable.Buffers
{
    public class IndexBuffer<T> : Buffer<T>
        where T : unmanaged
    {
        private readonly Format _format;

        public IndexBuffer(
            ID3D11RenderingEngine renderingEngine,
            T[] data,
            int sizeInBytes,
            Format format = Format.R16_UInt)
            : base(
                renderingEngine,
                data,
                new BufferDescription
                {
                    Usage = Vortice.Direct3D11.Usage.Default,
                    BindFlags = BindFlags.IndexBuffer,
                    OptionFlags = ResourceOptionFlags.None,
                    CpuAccessFlags = CpuAccessFlags.None,
                    StructureByteStride = sizeInBytes,
                    SizeInBytes = data.Length * sizeInBytes,
                }) => _format = format;

        public override void Bind() => _renderingEngine.DeviceContext
            .IASetIndexBuffer(_buffer, _format, 0);
    }
}