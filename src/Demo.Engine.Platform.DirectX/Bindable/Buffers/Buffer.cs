// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Bindable.Buffers;

public abstract class Buffer<T> : IBindable, IDisposable
    where T : unmanaged
{
    private bool _disposedValue;
    protected readonly ID3D11RenderingEngine _renderingEngine;
    protected readonly ID3D11Buffer _buffer;

    protected Buffer(
        ID3D11RenderingEngine renderingEngine,
        ref T data,
        BufferDescription bufferDescription)
    {
        _renderingEngine = renderingEngine;
        _buffer = _renderingEngine.Device.CreateBuffer(
            data,
            bufferDescription);

        SetDebugName(bufferDescription);
    }

    protected Buffer(
        ID3D11RenderingEngine renderingEngine,
        ReadOnlySpan<T> data,
        BufferDescription bufferDescription)
    {
        _renderingEngine = renderingEngine;
        _buffer = _renderingEngine.Device.CreateBuffer(
            data,
            bufferDescription);

        SetDebugName(bufferDescription);
    }

    private void SetDebugName(BufferDescription bufferDescription) =>
        _buffer.DebugName = $"[{GetType().Name}_{bufferDescription.ByteWidth}b] {Guid.NewGuid()}";

    /// <summary>
    /// Method that binds the buffer (and it's resources) to the rendering pipeline
    /// </summary>
    public abstract void Bind();

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _buffer.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable
}