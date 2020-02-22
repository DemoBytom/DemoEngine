using System;
using System.Runtime.InteropServices;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Models.Enums;
using Demo.Engine.Core.Models.Options;
using Demo.Tools.Common.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharpGen.Runtime.Win32;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Demo.Engine.DirectX
{
    public class RenderingEngine : IRenderingEngine
    {
        private readonly ID3D11Texture2D _backBuffer;
        private readonly ID3D11Device _device;
        private readonly ID3D11DeviceContext _deviceContext;
        private readonly IDXGIFactory1 _factory;
        private readonly FeatureLevel _featureLevel;
        private readonly ReadOnlyMemory<byte> _triangleVertexShader;
        private readonly ReadOnlyMemory<byte> _trianglePixelShader;
        private readonly ILogger<RenderingEngine> _logger;
        private readonly ID3D11RenderTargetView _renderTargetView;
        private readonly IDXGISwapChain _swapChain;
        private readonly IOptionsMonitor<RenderSettings> _formSettings;

        public RenderingEngine(
            ILogger<RenderingEngine> logger,
            IRenderingControl renderingForm,
            IOptionsMonitor<RenderSettings> renderSettings,
            IShaderCompiler shaderCompiler)
        {
            using var loggingContext = logger.LogScopeInitialization();
            _triangleVertexShader = shaderCompiler.CompileShader("Shaders/Triangle/TriangleVS.hlsl", ShaderStage.VertexShader);
            _trianglePixelShader = shaderCompiler.CompileShader("Shaders/Triangle/TrianglePS.hlsl", ShaderStage.PixelShader);
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

            Control.Show();
        }

        public IRenderingControl Control { get; }

        public void BeginScene() => BeginScene(new Color4(0, 0, 0, 1));

        public void BeginScene(Color4 color)
        {
            _deviceContext.ClearRenderTargetView(_renderTargetView, color);
        }

        public bool EndScene()
        {
            var result = _swapChain.Present(
                _formSettings.CurrentValue.VSync ? 1 : 0,
                PresentFlags.None
                );

            //temporary fix for a memory leak when creating them each frame
            _vertexBuffer?.Dispose();
            _vertexShader?.Dispose();
            _pixelShader?.Dispose();
            _inputLayout?.Dispose();

            return !result.Failure
                || result.Code != Vortice.DXGI.ResultCode.DeviceRemoved.Code;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            public float X { get; set; }
            public float Y { get; set; }

            public static readonly int SizeInBytes = Marshal.SizeOf<Vertex>();
        }

        private ID3D11Buffer? _vertexBuffer;
        private ID3D11VertexShader? _vertexShader;
        private ID3D11PixelShader? _pixelShader;
        private ID3D11InputLayout? _inputLayout;

        private readonly Vertex[] _triangleVertices = new Vertex[]
            {
                new Vertex{ X=0.0f,  Y=0.5f },
                new Vertex{ X=0.5f,  Y=-0.5f },
                new Vertex{ X=-0.5f, Y=-0.5f }
            };

        public void DrawTriangle()
        {
            _vertexBuffer = _device.CreateBuffer(
                _triangleVertices,
                new BufferDescription
                {
                    Usage = Vortice.Direct3D11.Usage.Default,
                    BindFlags = BindFlags.VertexBuffer,
                    OptionFlags = ResourceOptionFlags.None,
                    CpuAccessFlags = CpuAccessFlags.None,
                    StructureByteStride = Vertex.SizeInBytes,
                    SizeInBytes = _triangleVertices.Length * Vertex.SizeInBytes
                });

            //set Vertex buffer
            _deviceContext.IASetVertexBuffers(0, new VertexBufferView(_vertexBuffer, Vertex.SizeInBytes));

            unsafe
            {
                fixed (byte* ptr = &MemoryMarshal.GetReference(_triangleVertexShader.Span))
                {
                    _vertexShader = _device.CreateVertexShader((IntPtr)ptr, _triangleVertexShader.Length);
                }

                fixed (byte* ptr = &MemoryMarshal.GetReference(_trianglePixelShader.Span))
                {
                    _pixelShader = _device.CreatePixelShader((IntPtr)ptr, _trianglePixelShader.Length);
                }
            }

            _deviceContext.VSSetShader(_vertexShader);
            _deviceContext.PSSetShader(_pixelShader);

            _inputLayout = _device.CreateInputLayout(new[]
            {
                new InputElementDescription(
                    "position",
                    0,
                    Format.R32G32_Float,
                    0,
                    0,
                    InputClassification.PerVertexData,
                    0)
            }, _triangleVertexShader.ToArray());

            _deviceContext.IASetInputLayout(_inputLayout);

            //bind render target
            _deviceContext.OMSetRenderTargets(_renderTargetView);

            _deviceContext.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

            //viewport
            _deviceContext.RSSetViewport(new Viewport(Control.DrawingArea)
            {
                MinDepth = 0,
                MaxDepth = 1
            });

            _deviceContext.Draw(3, 0);
        }

        #region IDisposable Support

        /// <summary>
        /// To detect redundant calls
        /// </summary>
        private bool _disposedValue = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        /// <remarks>
        /// Uncomment the following line if the finalizer is overridden above. GC.SuppressFinalize(this);
        /// <para/>
        /// Override a finalizer only if Dispose(bool disposing) above has code to free unmanaged
        /// resources. ~RenderingEngine() { // Do not change this code. Put cleanup code in Dispose(bool
        /// disposing) above. Dispose(false);
        /// </remarks>
        void IDisposable.Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _renderTargetView?.Dispose();
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
    }

    #endregion IDisposable Support
}