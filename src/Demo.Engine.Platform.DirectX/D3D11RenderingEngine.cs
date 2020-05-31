using System;
using System.Collections.Generic;
using System.Numerics;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Models.Options;
using Demo.Engine.Platform.DirectX.Interfaces;
using Demo.Tools.Common.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharpGen.Runtime.Win32;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX
{
    public class D3D11RenderingEngine : ID3D11RenderingEngine
    {
        private readonly ID3D11Texture2D _backBuffer;
        private readonly ID3D11Device _device;
        private readonly ID3D11DeviceContext _deviceContext;
        private readonly IDXGIFactory1 _factory;
        private readonly FeatureLevel _featureLevel;

        private readonly ILogger<D3D11RenderingEngine> _logger;
        private readonly ID3D11RenderTargetView _renderTargetView;
        private readonly ID3D11DepthStencilView _depthStencilView;
        private readonly ID3D11DepthStencilState _depthStencilState;
        private readonly ID3D11Texture2D _depthStencilTexture;
        private readonly IDXGISwapChain _swapChain;
        private readonly IOptionsMonitor<RenderSettings> _formSettings;

        private bool _disposedValue = false;

        public ID3D11Device Device => _device;
        public ID3D11DeviceContext DeviceContext => _deviceContext;

        public D3D11RenderingEngine(
            ILogger<D3D11RenderingEngine> logger,
            IRenderingControl renderingForm,
            IOptionsMonitor<RenderSettings> renderSettings)
        {
            using var loggingContext = logger.LogScopeInitialization();
            _logger = logger;
            _formSettings = renderSettings;

            Control = renderingForm;

            if (DXGI.CreateDXGIFactory1(out _factory).Failure)
            {
                throw new InvalidOperationException("Cannot create {IDXGIFactory1} instance!");
            }

            D3D11.D3D11CreateDevice(
                null,
                DriverType.Hardware,
                DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug,
                new[]
                {
                    FeatureLevel.Level_11_1,
                    FeatureLevel.Level_11_0
                },
                out _device,
                out _featureLevel,
                out _deviceContext
                );

            _logger.LogDebug("Initiated device with {featureLeve}", _featureLevel);

            var swapChainDescription = new SwapChainDescription
            {
                BufferCount = 1,
                BufferDescription = new ModeDescription(
                    _formSettings.CurrentValue.Width,
                    _formSettings.CurrentValue.Height,
                    Format.B8G8R8A8_UNorm),
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

            var rd = new RasterizerDescription
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Back,
                FrontCounterClockwise = true,
                DepthBias = 0,
                SlopeScaledDepthBias = 0.0f,
                DepthBiasClamp = 0.0f,
                DepthClipEnable = true,
                ScissorEnable = false,
                MultisampleEnable = false,
                AntialiasedLineEnable = false,
            };

            _deviceContext.RSSetState(_device.CreateRasterizerState(rd));

            _depthStencilState = _device.CreateDepthStencilState(new DepthStencilDescription
            {
                DepthEnable = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthFunc = ComparisonFunction.Less,
            });
            _deviceContext.OMSetDepthStencilState(_depthStencilState);

            _depthStencilTexture = _device.CreateTexture2D(new Texture2DDescription
            {
                Width = _formSettings.CurrentValue.Width,
                Height = _formSettings.CurrentValue.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D32_Float,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Vortice.Direct3D11.Usage.Default,
                BindFlags = BindFlags.DepthStencil
            });

            _depthStencilView = _device.CreateDepthStencilView(_depthStencilTexture, new DepthStencilViewDescription
            {
                Format = Format.D32_Float,
                ViewDimension = DepthStencilViewDimension.Texture2D,
                Texture2D = new Texture2DDepthStencilView { MipSlice = 0 }
            });

            Control.Show();
        }

        public IRenderingControl Control { get; }

        private const int FOV = 90;
        private const float FOV_RAD = FOV * (MathF.PI / 180);

        /* TODO separate into view and projection matrices, and move View matrix to a Camera
         * Projection can (should be?) calculated once, and left there till it needs to be changed
         * */

        public Matrix4x4 ViewProjectionMatrix => Matrix4x4.Transpose(
                // View matrix - Camera
                Matrix4x4.CreateLookAt(new Vector3(0.0f, 0.0f, 4.0f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitY)
                // Projection matrix - perspective
                //* Matrix4x4.CreatePerspective(1, Control.DrawHeight / (float)Control.DrawWidth, 0.1f, 10f));
                * Matrix4x4.CreatePerspectiveFieldOfView(
                    FOV_RAD,
                    (float)Control.DrawWidth / Control.DrawHeight,
                    0.1f,
                    10f));

        public void BeginScene() => BeginScene(new Color4(0, 0, 0, 1));

        public void BeginScene(Color4 color)
        {
            _deviceContext.ClearRenderTargetView(_renderTargetView, color);
            _deviceContext.ClearDepthStencilView(_depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        public bool EndScene()
        {
            var result = _swapChain.Present(
                _formSettings.CurrentValue.VSync ? 1 : 0,
                PresentFlags.None
                );

            return !result.Failure
                || result.Code != Vortice.DXGI.ResultCode.DeviceRemoved.Code;
        }

        public void Draw(IEnumerable<IDrawable> drawables)
        {
            //bind render target
            _deviceContext.OMSetRenderTargets(_renderTargetView, _depthStencilView);
            foreach (var drawable in drawables)
            {
                drawable.Draw();
            }
        }

        #region IDisposable Support

        void IDisposable.Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                    _renderTargetView?.Dispose();
                    _depthStencilView?.Dispose();
                    _depthStencilState?.Dispose();
                    _depthStencilTexture?.Dispose();
                    _backBuffer?.Dispose();
                    _deviceContext?.ClearState();
                    _deviceContext?.Flush();
                    _deviceContext?.Dispose();
                    _device?.Dispose();
                    RawBool fullscreen = false;
                    _swapChain?.GetFullscreenState(out fullscreen);
                    if (fullscreen == true)
                    {
                        _swapChain?.SetFullscreenState(false);
                    }

                    _swapChain?.Dispose();
                    _factory?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}