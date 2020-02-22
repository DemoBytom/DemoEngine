using System;
using System.IO;
using System.Runtime.InteropServices;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
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
        private readonly byte[] _triangleVertexShader;
        private readonly byte[] _trianglePixelShader;
        private readonly ILogger<RenderingEngine> _logger;
        private readonly ID3D11RenderTargetView _renderTargetView;
        private readonly IDXGISwapChain _swapChain;
        private readonly IOptionsMonitor<RenderSettings> _formSettings;

        public RenderingEngine(
            ILogger<RenderingEngine> logger,
            IRenderingControl renderingForm,
            IOptionsMonitor<RenderSettings> renderSettings)
        {
            using var loggingContext = logger.LogScopeInitialization();
            _triangleVertexShader = RenderShader("Shaders/Triangle/TriangleVS.hlsl", ShaderStage.VertexShader);
            _trianglePixelShader = RenderShader("Shaders/Triangle/TrianglePS.hlsl", ShaderStage.PixelShader);
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

        public void DrawTriangle()
        {
            var vertices = new Vertex[]
            {
                new Vertex{ X=0.0f,  Y=0.5f },
                new Vertex{ X=0.5f,  Y=-0.5f },
                new Vertex{ X=-0.5f, Y=-0.5f }
            };

            var vertexBuffer = _device.CreateBuffer<Vertex>(
                vertices,
                new BufferDescription
                {
                    Usage = Vortice.Direct3D11.Usage.Default,
                    BindFlags = BindFlags.VertexBuffer,
                    OptionFlags = ResourceOptionFlags.None,
                    CpuAccessFlags = CpuAccessFlags.None,
                    StructureByteStride = Vertex.SizeInBytes,
                    SizeInBytes = vertices.Length * Vertex.SizeInBytes
                }
                );

            //set Vertex buffer
            _deviceContext.IASetVertexBuffers(0, new VertexBufferView(vertexBuffer, Vertex.SizeInBytes));

            var vertexShader = _device.CreateVertexShader(_triangleVertexShader);
            var pixelShader = _device.CreatePixelShader(_trianglePixelShader);

            _deviceContext.VSSetShader(vertexShader);
            _deviceContext.PSSetShader(pixelShader);

            //set input (vertex) layout
            var inputElementDesc = new[]
            {
                new InputElementDescription(
                    "position",
                    0,
                    Format.R32G32_Float,
                    0,
                    0,
                    InputClassification.PerVertexData,
                    0)
            };
            var inputLayout = _device.CreateInputLayout(inputElementDesc, _triangleVertexShader);
            _deviceContext.IASetInputLayout(inputLayout);

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

        private byte[] RenderShader(string path, ShaderStage shaderStage)
        {
            var shader = File.ReadAllText(path);

            var shaderProfile = $"{GetShaderProfile(shaderStage)}_5_0";
            var compileResult = Vortice.D3DCompiler.Compiler.Compile(
                shader,
                "main",
                "TriangleVS.hlsl",
                shaderProfile,
                out var blob,
                out var errorBlob
                );

            if (compileResult.Failure)
            {
                throw new Exception(errorBlob?.ConvertToString());
            }
            else
            {
                return blob.GetBytes();
            }
        }

        private static string GetShaderProfile(ShaderStage stage) => stage switch
        {
            ShaderStage.VertexShader => "vs",
            ShaderStage.HullShader => "hs",
            ShaderStage.DomainShader => "ds",
            ShaderStage.GeometryShader => "gs",
            ShaderStage.PixelShader => "ps",
            ShaderStage.ComputeShader => "cs",
            _ => string.Empty,
        };

        public enum ShaderStage : uint
        {
            VertexShader,
            HullShader,
            DomainShader,
            GeometryShader,
            PixelShader,
            ComputeShader,
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