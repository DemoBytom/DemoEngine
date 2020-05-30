using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Bindable.Buffers
{
    /// <summary>
    /// Constant buffer that can be bound to the rendering pipeline
    /// </summary>
    /// <typeparam name="T">Data contained in the buffer</typeparam>
    public abstract class ConstantBuffer<T> : Buffer<T>, IUpdatable
        where T : unmanaged
    {
        protected ConstantBuffer(ID3D11RenderingEngine renderingEngine, ref T data)
            : base(renderingEngine, ref data, new BufferDescription
            {
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                OptionFlags = ResourceOptionFlags.None,
                CpuAccessFlags = CpuAccessFlags.Write,
                StructureByteStride = 0,
                SizeInBytes = Marshal.SizeOf<T>(),
            })
        {
        }

        /// <summary>
        /// Method used to update the buffer with new data
        /// </summary>
        /// <param name="data"></param>
        public void Update(ref T data)
        {
            unsafe
            {
                var ms = _renderingEngine.DeviceContext.Map(
                    _buffer,
                    0,
                    MapMode.WriteDiscard,
                    MapFlags.None);
                Unsafe.CopyBlockUnaligned(
                    (void*)ms.DataPointer,
                    Unsafe.AsPointer(ref data),
                    (uint)MatricesBuffer.SizeInBytes);
                _renderingEngine.DeviceContext.Unmap(_buffer, 0);
            }
        }

        public void Update() => OnUpdate?.Invoke(this);

        public event ConstantBufferEventHandler? OnUpdate;

        public delegate void ConstantBufferEventHandler(ConstantBuffer<T> buffer);
    }
}