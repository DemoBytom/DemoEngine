using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Models.Enums;
using Demo.Engine.Platform.DirectX.Bindable;
using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX.Models
{
    public class Cube : IDisposable, ICube
    {
        private readonly IRenderingEngine _renderingEngine;
        private readonly ID3D11Device _device;
        private readonly ID3D11DeviceContext _deviceContext;

        private readonly ReadOnlyCollection<IBindable> _bindables;
        private readonly ReadOnlyCollection<IUpdatable> _updatables;
        private bool _disposedValue = false;

        private readonly Vertex[] _triangleVertices = new Vertex[]
        {
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
            //Cube
            4,5,6, 6,5,7,
            5,1,7, 7,1,3,
            1,0,3, 3,0,2,
            0,4,2, 2,4,6,
            6,7,2, 2,7,3,
            5,4,1, 1,4,0
        };

        private readonly ReadOnlyMemory<byte> _triangleVertexShader;
        private readonly ReadOnlyMemory<byte> _trianglePixelShader;

        private readonly ID3D11Buffer? _vertexBuffer;
        private readonly ID3D11Buffer? _indexBuffer;
        private readonly ID3D11VertexShader? _vertexShader;
        private readonly ID3D11PixelShader? _pixelShader;
        private readonly ID3D11InputLayout? _inputLayout;

        public Cube(
            ID3DRenderingEngine renderingEngine,
            IShaderCompiler shaderCompiler)
        {
            _renderingEngine = renderingEngine;
            _device = renderingEngine.Device;
            _deviceContext = renderingEngine.DeviceContext;

            _triangleVertexShader = shaderCompiler.CompileShader("Shaders/Triangle/TriangleVS.hlsl", ShaderStage.VertexShader);
            _trianglePixelShader = shaderCompiler.CompileShader("Shaders/Triangle/TrianglePS.hlsl", ShaderStage.PixelShader);

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

            //VertexBuffer
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

            //IndexBuffer
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

            _matricesBuffer = new MatricesBuffer(Matrix4x4.Identity, Matrix4x4.Identity);
            var matricesConstantBuffer = new VSConstantBuffer<MatricesBuffer>(
                renderingEngine,
                ref _matricesBuffer);

            var colors = new CubeFacesColors(
                new Color4(255, 000, 000, 255),
                new Color4(000, 255, 000, 255),
                new Color4(000, 000, 255, 255),
                new Color4(000, 125, 125, 255),
                new Color4(125, 125, 000, 255),
                new Color4(125, 000, 125, 255));
            var colorsConstantBuffer = new PSConstantBuffer<CubeFacesColors>(
                renderingEngine,
                ref colors);

            matricesConstantBuffer.OnUpdate += (o, _) => o.Update(ref _matricesBuffer);

            _bindables = new ReadOnlyCollectionBuilder<IBindable>
            {
                matricesConstantBuffer,
                colorsConstantBuffer,
            }.ToReadOnlyCollection();

            _updatables = new ReadOnlyCollectionBuilder<IUpdatable>
            {
                matricesConstantBuffer
            }.ToReadOnlyCollection();
        }

        private Vector3 _position;
        private float _rotationAngleInRadians;
        private MatricesBuffer _matricesBuffer;

        public void Update(Vector3 position, float rotationAngleInRadians)
        {
            _position = position;
            _rotationAngleInRadians = rotationAngleInRadians;

            //Model to world transformation(s)
            var worldMatrix = Matrix4x4.Transpose(
                 Matrix4x4.Identity
                 * Matrix4x4.CreateRotationZ(rotationAngleInRadians)
                 * Matrix4x4.CreateRotationY(rotationAngleInRadians)
                 * Matrix4x4.CreateRotationX(rotationAngleInRadians)
                 * Matrix4x4.CreateTranslation(position)
                 );

            const int FOV = 90;
            const float FOV_RAD = FOV * (MathF.PI / 180);

            var viewProjectionMatrix = Matrix4x4.Transpose(
                // View matrix - Camera
                Matrix4x4.CreateLookAt(new Vector3(0.0f, 0.0f, 4.0f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitY)
                // Projection matrix - perspective
                //* Matrix4x4.CreatePerspective(1, Control.DrawHeight / (float)Control.DrawWidth, 0.1f, 10f));
                * Matrix4x4.CreatePerspectiveFieldOfView(FOV_RAD, (float)_renderingEngine.Control.DrawWidth / _renderingEngine.Control.DrawHeight, 0.1f, 10f));
            _matricesBuffer = new MatricesBuffer(worldMatrix, viewProjectionMatrix);
            foreach (var updatable in _updatables)
            {
                updatable.Update();
            }
        }

        public void Draw()
        {
            foreach (var bindable in _bindables)
            {
                bindable.Bind();
            }
            //set Vertex buffer
            _deviceContext.IASetVertexBuffers(0, new VertexBufferView(_vertexBuffer, Vertex.SizeInBytes));
            //set Index buffer
            _deviceContext.IASetIndexBuffer(_indexBuffer, Format.R16_UInt, 0);

            _deviceContext.VSSetShader(_vertexShader);
            _deviceContext.PSSetShader(_pixelShader);

            _deviceContext.IASetInputLayout(_inputLayout);

            _deviceContext.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

            //viewport
            _deviceContext.RSSetViewport(new Viewport(_renderingEngine.Control.DrawingArea)
            {
                MinDepth = 0,
                MaxDepth = 1
            });

            _deviceContext.DrawIndexed(_triangleIndices.Length, 0, 0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _vertexBuffer?.Dispose();
                    _indexBuffer?.Dispose();
                    _vertexShader?.Dispose();
                    _pixelShader?.Dispose();
                    _inputLayout?.Dispose();

                    foreach (var disposable in _bindables.OfType<IDisposable>())
                    {
                        disposable.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Cube()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
    }
}