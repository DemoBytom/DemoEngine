using System;
using System.Numerics;
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

namespace Demo.Engine.Platform.DirectX
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

            var rd = new Vortice.Direct3D11.RasterizerDescription
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
            _indexBuffer?.Dispose();
            _constantBuffer?.Dispose();
            _vertexShader?.Dispose();
            _pixelShader?.Dispose();
            _inputLayout?.Dispose();

            return !result.Failure
                || result.Code != Vortice.DXGI.ResultCode.DeviceRemoved.Code;
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct Vertex
        {
            public Vertex(
                float x, float y, float z,
                byte r, byte g, byte b, byte a)
                : this(new Vector3(x, y, z), new Color(r, g, b, a)) { }

            public Vertex(Vector3 position, Color color) =>
                (Position, Color) = (position, color);

            public Vector3 Position { get; }
            public Color Color { get; }

            public static readonly int SizeInBytes = Marshal.SizeOf<Vertex>();
            public static readonly int PositionSizeInBytes = Marshal.SizeOf<Vector3>();
            public static readonly int ColorSizeInBytes = Marshal.SizeOf<Color>();
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct MatricesBuffer
        {
            public MatricesBuffer(
                Matrix4x4 modelToWorldMatrix,
                Matrix4x4 viewProjectionMatrix) =>
                (ModelToWorldMatrix, ViewProjectionMatrix) = (modelToWorldMatrix, viewProjectionMatrix);

            public Matrix4x4 ModelToWorldMatrix { get; }
            public Matrix4x4 ViewProjectionMatrix { get; }

            public static readonly int SizeInBytes = Marshal.SizeOf<MatricesBuffer>();
        }

        public readonly struct CubeFacesColors
        {
            public CubeFacesColors(Color4 face1, Color4 face2, Color4 face3, Color4 face4, Color4 face5, Color4 face6)
            {
                Face1 = face1;
                Face2 = face2;
                Face3 = face3;
                Face4 = face4;
                Face5 = face5;
                Face6 = face6;
            }

            public Color4 Face1 { get; }
            public Color4 Face2 { get; }
            public Color4 Face3 { get; }
            public Color4 Face4 { get; }
            public Color4 Face5 { get; }
            public Color4 Face6 { get; }

            public static int SizeInBytes = Marshal.SizeOf<CubeFacesColors>();
        }

        private ID3D11Buffer? _vertexBuffer;
        private ID3D11Buffer? _indexBuffer;
        private ID3D11Buffer? _constantBuffer;
        private ID3D11Buffer? _cubeFacesColorsBuffer;
        private ID3D11VertexShader? _vertexShader;
        private ID3D11PixelShader? _pixelShader;
        private ID3D11InputLayout? _inputLayout;

        private readonly Vertex[] _triangleVertices = new Vertex[]
        {
            //Triangles
            //new Vertex( 0.0f,  0.75f, -0.1f, 255, 000, 000, 255),
            //new Vertex( 0.5f,  0.0f,  -0.1f, 255, 255, 255, 255),
            //new Vertex(-0.5f,  0.0f,  -0.1f, 255, 255, 255, 255),
            //new Vertex( 0.0f, -0.75f, -0.1f, 000, 000, 255, 255),

            //Cube
            new Vertex( -1.0f, -1.0f, -1.0f, 255, 000, 000, 255  ),
            new Vertex(  1.0f, -1.0f, -1.0f, 125, 125, 000, 255  ),
            new Vertex( -1.0f,  1.0f, -1.0f, 000, 125, 125, 255  ),
            new Vertex(  1.0f,  1.0f, -1.0f, 000, 000, 255, 255  ),
            new Vertex( -1.0f, -1.0f,  1.0f, 125, 000, 125, 255  ),
            new Vertex(  1.0f, -1.0f,  1.0f, 000, 255, 000, 255  ),
            new Vertex( -1.0f,  1.0f,  1.0f, 000, 000, 000, 255  ),
            new Vertex(  1.0f,  1.0f,  1.0f, 000, 000, 255, 255  ),
        };

        private readonly ushort[] _triangleIndices = new ushort[]
        {
            //Trianlges
            //0, 1, 2,
            //2, 1, 3,
            //0, 2, 1,
            //1, 2, 3,
            //Cube
            4,5,6, 6,5,7,
            5,1,7, 7,1,3,
            1,0,3, 3,0,2,
            0,4,2, 2,4,6,
            6,7,2, 2,7,3,
            5,4,1, 1,4,0
        };

        public void DrawCube(float angleInRadians)
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

            _indexBuffer = _device.CreateBuffer(
                _triangleIndices,
                new BufferDescription
                {
                    Usage = Vortice.Direct3D11.Usage.Default,
                    BindFlags = BindFlags.IndexBuffer,
                    OptionFlags = ResourceOptionFlags.None,
                    CpuAccessFlags = CpuAccessFlags.None,
                    StructureByteStride = sizeof(ushort),
                    SizeInBytes = sizeof(ushort) * _triangleIndices.Length,
                });

            //Model to world transformation(s)
            var worldMatrix = Matrix4x4.Transpose(
                Matrix4x4.Identity
                * Matrix4x4.CreateRotationZ(angleInRadians)
                * Matrix4x4.CreateRotationY(angleInRadians)
                * Matrix4x4.CreateRotationX(angleInRadians)
                * Matrix4x4.CreateTranslation(0f, 0f, -0.0f)

                );
            const int FOV = 90;
            var fovRad = FOV * (MathF.PI / 180);

            var viewProjectionMatrix = Matrix4x4.Transpose(
                // View matrix - Camera
                Matrix4x4.CreateLookAt(new Vector3(0.0f, 0.0f, 4.0f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitY)
                // Projection matrix - perspective
                //* Matrix4x4.CreatePerspective(1, Control.DrawHeight / (float)Control.DrawWidth, 0.1f, 10f));
                * Matrix4x4.CreatePerspectiveFieldOfView(fovRad, (float)Control.DrawWidth / Control.DrawHeight, 0.1f, 10f));
            var matricesBuffer = new MatricesBuffer(worldMatrix, viewProjectionMatrix);

            //constant buffer
            _constantBuffer = _device.CreateBuffer(
                ref matricesBuffer,
                new BufferDescription
                {
                    Usage = Vortice.Direct3D11.Usage.Dynamic,
                    BindFlags = BindFlags.ConstantBuffer,
                    OptionFlags = ResourceOptionFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    StructureByteStride = 0,
                    SizeInBytes = MatricesBuffer.SizeInBytes,
                });
            //pixel shader constant buffers (cube colors)
            var colors = new CubeFacesColors(
                new Color4(255, 000, 000, 255),
                new Color4(000, 255, 000, 255),
                new Color4(000, 000, 255, 255),
                new Color4(000, 125, 125, 255),
                new Color4(125, 125, 000, 255),
                new Color4(125, 000, 125, 255));
            _cubeFacesColorsBuffer = _device.CreateBuffer(
                ref colors,
                new BufferDescription
                {
                    Usage = Vortice.Direct3D11.Usage.Dynamic,
                    BindFlags = BindFlags.ConstantBuffer,
                    OptionFlags = ResourceOptionFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    StructureByteStride = Marshal.SizeOf<Color4>(),
                    SizeInBytes = CubeFacesColors.SizeInBytes
                });

            //set Vertex buffer
            _deviceContext.IASetVertexBuffers(0, new VertexBufferView(_vertexBuffer, Vertex.SizeInBytes));
            //set Index buffer
            _deviceContext.IASetIndexBuffer(_indexBuffer, Format.R16_UInt, 0);
            //set Vertex Shader Constant buffer
            _deviceContext.VSSetConstantBuffers(0, _constantBuffer);
            //set PixelShader Constatn buffer
            _deviceContext.PSSetConstantBuffers(0, _cubeFacesColorsBuffer);
            unsafe
            {
                fixed (byte* ptr = _triangleVertexShader.Span)
                {
                    _vertexShader = _device.CreateVertexShader((IntPtr)ptr, _triangleVertexShader.Length);
                }

                fixed (byte* ptr = _trianglePixelShader.Span)
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
                    Format.R32G32B32_Float,
                    0,
                    0,
                    InputClassification.PerVertexData,
                    0),
                new InputElementDescription(
                    "color",
                    0,
                    Format.R8G8B8A8_UNorm,
                    Vertex.PositionSizeInBytes,
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

            _deviceContext.DrawIndexed(_triangleIndices.Length, 0, 0);
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
                    _vertexBuffer?.Dispose();
                    _indexBuffer?.Dispose();
                    _constantBuffer?.Dispose();
                    _vertexShader?.Dispose();
                    _pixelShader?.Dispose();
                    _inputLayout?.Dispose();

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