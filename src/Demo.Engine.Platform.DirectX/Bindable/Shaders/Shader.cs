// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System;
using Demo.Engine.Platform.DirectX.Interfaces;
using Demo.Engine.Platform.DirectX.Shaders;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Bindable.Shaders;

public abstract class Shader<TShaderType> : IBindable, IDisposable
    where TShaderType : ID3D11DeviceChild
{
    private bool _disposedValue;
    protected TShaderType _shader;
    public ReadOnlyMemory<byte> CompiledShader { get; }
    protected readonly ID3D11RenderingEngine _renderingEngine;

    protected Shader(
        CompiledS compiledS,
        Func<ID3D11Device, (IntPtr shaderPointer, int shaderLen), TShaderType> func,
        ID3D11RenderingEngine renderingEngine)
    {
        CompiledShader = compiledS.CompiledShader;
        _renderingEngine = renderingEngine;

        unsafe
        {
            fixed (byte* ptr = CompiledShader.Span)
            {
                _shader = func(_renderingEngine.Device, ((IntPtr)ptr, CompiledShader.Length));
            }
        }

        _shader.DebugName = $"[{GetType().Name}_{CompiledShader.Length}b {Guid.NewGuid()}";
    }

    public abstract void Bind();

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _shader.Dispose();
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