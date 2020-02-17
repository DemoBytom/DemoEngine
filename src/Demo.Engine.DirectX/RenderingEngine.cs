using System;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Microsoft.Extensions.Logging;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Demo.Engine.DirectX
{
    public class RenderingEngine : IRenderingEngine
    {
        private readonly IDXGIFactory1 _factory;
        private readonly ID3D11Device _device;
        private readonly FeatureLevel _featureLevel;
        private readonly ID3D11DeviceContext _deviceContext;
        private readonly IDXGISwapChain _swapChain;
        private readonly ID3D11Texture2D _backBuffer;
        private readonly ID3D11RenderTargetView _renderTargetView;

        public RenderingEngine(
            ILogger<RenderingEngine> logger,
            IRenderingControl renderingForm)
        {
            _logger = logger;
            _logger.LogDebug("{class} initialization {state}", nameof(RenderingEngine), "started");

            Control = renderingForm;

            if (DXGI.CreateDXGIFactory1(out _factory).Failure)
            {
                throw new InvalidOperationException("Cannot create {IDXGIFactory1} instance!");
            }

            D3D11.D3D11CreateDevice(
                null,
                DriverType.Hardware,
                DeviceCreationFlags.BgraSupport,
                new[]
                {
                    FeatureLevel.Level_11_1,
                    FeatureLevel.Level_11_0
                },
                out _device,
                out _featureLevel,
                out _deviceContext
                );

            var swapChainDescription = new SwapChainDescription
            {
                BufferCount = 2,
                BufferDescription = new ModeDescription(1024, 768, Format.B8G8R8A8_UNorm),
                IsWindowed = true,
                OutputWindow = Control.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Vortice.DXGI.Usage.RenderTargetOutput
            };

            _swapChain = _factory.CreateSwapChain(_device, swapChainDescription);
            _factory.MakeWindowAssociation(Control.Handle, WindowAssociationFlags.IgnoreAltEnter);

            _backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
            _renderTargetView = _device.CreateRenderTargetView(_backBuffer);

            Control.Show();

            _logger.LogDebug("{class} initialization {state}", nameof(RenderingEngine), "completed");
        }

        public IRenderingControl Control { get; }

        public void BeginScene(Color4 color)
        {
            _deviceContext.ClearRenderTargetView(_renderTargetView, color);
        }

        public bool EndScene()
        {
            var vSync = true;
            var result = _swapChain.Present(
                vSync ? 1 : 0,
                PresentFlags.None
                );

            return !result.Failure
                || result.Code != Vortice.DXGI.ResultCode.DeviceRemoved.Code;
        }

        #region IDisposable Support

        /// <summary>
        /// To detect redundant calls
        /// </summary>
        private bool _disposedValue = false;

        private readonly ILogger<RenderingEngine> _logger;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _renderTargetView.Dispose();
                    _backBuffer.Dispose();
                    _deviceContext.ClearState();
                    _deviceContext.Flush();
                    _deviceContext.Dispose();
                    _device.Dispose();
                    _swapChain.Dispose();
                    _factory.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RenderingEngine() { // Do not change this code. Put cleanup code in Dispose(bool
        // disposing) above. Dispose(false); }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above. GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}