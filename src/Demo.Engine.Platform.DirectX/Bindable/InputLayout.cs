// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System;
using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Bindable
{
    public class InputLayout : IBindable, IDisposable
    {
        private bool _disposedValue;
        private readonly ID3D11RenderingEngine _renderingEngine;
        private readonly ID3D11InputLayout _inputLayout;

        public InputLayout(
            ID3D11RenderingEngine renderingEngine,
            InputElementDescription[] inputElementDescriptions,
            ReadOnlyMemory<byte> compiledShader)
        {
            _renderingEngine = renderingEngine;
            _inputLayout = _renderingEngine.Device.CreateInputLayout(
                inputElementDescriptions,
                compiledShader.ToArray());
        }

        public void Bind() => _renderingEngine.DeviceContext.IASetInputLayout(_inputLayout);

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
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