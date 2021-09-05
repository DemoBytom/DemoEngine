// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Models.Enums;
using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Bindable.Shaders
{
    public abstract class Shader<TShaderType> : IBindable, IDisposable
        where TShaderType : ID3D11DeviceChild
    {
        private bool _disposedValue;
        protected TShaderType _shader;
        public ReadOnlyMemory<byte> CompiledShader { get; }
        protected readonly ID3D11RenderingEngine _renderingEngine;

        protected Shader(
            string path,
            ShaderStage shaderStage,
            Func<ID3D11Device, (IntPtr shaderPointer, int shaderLen), TShaderType> func,
            IShaderCompiler shaderCompiler,
            ID3D11RenderingEngine renderingEngine)
        {
            CompiledShader = shaderCompiler.CompileShader(path, shaderStage);
            _renderingEngine = renderingEngine;

            unsafe
            {
                fixed (byte* ptr = CompiledShader.Span)
                {
                    _shader = func(_renderingEngine.Device, ((IntPtr)ptr, CompiledShader.Length));
                }
            }
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
}