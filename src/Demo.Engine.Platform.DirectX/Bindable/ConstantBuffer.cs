using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Bindable
{
    /// <summary>
    /// Constant buffer tat can be bound to the rendering pipeline
    /// </summary>
    /// <typeparam name="T">Data contained in the buffer</typeparam>
    public abstract class ConstantBuffer<T> : IBindable, IUpdatable, IDisposable
        where T : unmanaged
    {
        protected readonly ID3DRenderingEngine _renderingEngine;
        protected readonly ID3D11Buffer _constantBuffer;

        private bool _disposedValue = false;

        protected ConstantBuffer(ID3DRenderingEngine renderingEngine, ref T data)
        {
            _renderingEngine = renderingEngine;

            _constantBuffer = _renderingEngine.Device.CreateBuffer(
                ref data,
                new BufferDescription
                {
                    Usage = Usage.Dynamic,
                    BindFlags = BindFlags.ConstantBuffer,
                    OptionFlags = ResourceOptionFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    StructureByteStride = 0,
                    SizeInBytes = Marshal.SizeOf<T>(),
                });
        }

#pragma warning disable RCS1079 // Throwing of new NotImplementedException.

        public virtual void Bind() => throw new NotImplementedException();

#pragma warning restore RCS1079 // Throwing of new NotImplementedException.

        /// <summary>
        /// Method used to update the buffer with new data
        /// </summary>
        /// <param name="data"></param>
        public void Update(ref T data)
        {
            unsafe
            {
                var ms = _renderingEngine.DeviceContext.Map(
                    _constantBuffer,
                    0,
                    MapMode.WriteDiscard,
                    MapFlags.None);
                Unsafe.CopyBlockUnaligned(
                    (void*)ms.DataPointer,
                    Unsafe.AsPointer(ref data),
                    (uint)MatricesBuffer.SizeInBytes);
                _renderingEngine.DeviceContext.Unmap(_constantBuffer, 0);
            }
        }

        public void Update() => OnUpdate?.Invoke(this, EventArgs.Empty);

        public event ConstantBufferEventHandler? OnUpdate;

        public delegate void ConstantBufferEventHandler(ConstantBuffer<T> buffer, EventArgs e);

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _constantBuffer.Dispose();
                }

                _disposedValue = true;
            }
        }

        // ~ConstantBuffer()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}