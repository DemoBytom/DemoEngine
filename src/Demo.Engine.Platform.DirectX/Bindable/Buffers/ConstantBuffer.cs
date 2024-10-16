// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.CompilerServices;
using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Bindable.Buffers;

/// <summary>
/// Constant buffer that can be bound to the rendering pipeline
/// </summary>
/// <typeparam name="T">Data contained in the buffer</typeparam>
public abstract class ConstantBuffer<T> : Buffer<T>, IUpdatable<T>
    where T : unmanaged
{
    protected ConstantBuffer(ID3D11RenderingEngine renderingEngine, ref T data)
        : base(renderingEngine, ref data, new BufferDescription
        {
            Usage = ResourceUsage.Dynamic,
            BindFlags = BindFlags.ConstantBuffer,
            MiscFlags = ResourceOptionFlags.None,
            CPUAccessFlags = CpuAccessFlags.Write,
            StructureByteStride = 0,
            ByteWidth = (uint)Unsafe.SizeOf<T>(),
        })
    {
    }

    /// <summary>
    /// Method used to update the buffer with new data
    /// </summary>
    /// <param name="data"></param>
    public void Update(in T data)
    {
        unsafe
        {
            var ms = _renderingEngine.DeviceContext.Map(
                _buffer,
                0,
                MapMode.WriteDiscard,
                MapFlags.None);

            fixed (void* dataPtr = &data)
            {
                Unsafe.CopyBlock(
                    (void*)ms.DataPointer,
                    dataPtr,
                    (uint)MatricesBuffer.SizeInBytes);
            }
            _renderingEngine.DeviceContext.Unmap(_buffer, 0);
        }
    }
}