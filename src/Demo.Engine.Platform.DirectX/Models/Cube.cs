using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Platform.DirectX.Bindable;
using Demo.Engine.Platform.DirectX.Bindable.Buffers;
using Demo.Engine.Platform.DirectX.Bindable.Shaders;
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

        public Cube(
            ID3D11RenderingEngine renderingEngine,
            IShaderCompiler shaderCompiler)
        {
            _renderingEngine = renderingEngine;
            _device = renderingEngine.Device;
            _deviceContext = renderingEngine.DeviceContext;

            var vertexShader = new VertexShader(
                "Shaders/Triangle/TriangleVS.hlsl",
                shaderCompiler,
                renderingEngine);
            var pixelShader = new PixelShader(
                "Shaders/Triangle/TrianglePS.hlsl",
                shaderCompiler,
                renderingEngine);

            //VertexBuffer
            var vertexBuffer = new VertexBuffer<Vertex>(
                renderingEngine,
                _triangleVertices,
                Vertex.SizeInBytes);

            //IndexBuffer
            var indexBuffer = new IndexBuffer<ushort>(
                renderingEngine,
                _triangleIndices,
                sizeof(ushort));
            var inputLayout = new InputLayout(
                renderingEngine,
                new[]
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
                }, vertexShader.CompiledShader);

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

            matricesConstantBuffer.OnUpdate += o => o.Update(ref _matricesBuffer);

            _bindables = new ReadOnlyCollectionBuilder<IBindable>
            {
                matricesConstantBuffer,
                colorsConstantBuffer,
                vertexShader,
                pixelShader,
                vertexBuffer,
                indexBuffer,
                inputLayout,
            }.ToReadOnlyCollection();

            _updatables = new ReadOnlyCollectionBuilder<IUpdatable>
            {
                matricesConstantBuffer
            }.ToReadOnlyCollection();
        }

        private MatricesBuffer _matricesBuffer;

        public void Update(Vector3 position, float rotationAngleInRadians)
        {
            //Model to world transformation(s)
            var worldMatrix = Matrix4x4.Transpose(
                 Matrix4x4.Identity
                 * Matrix4x4.CreateRotationZ(rotationAngleInRadians)
                 * Matrix4x4.CreateRotationY(rotationAngleInRadians)
                 * Matrix4x4.CreateRotationX(rotationAngleInRadians)
                 * Matrix4x4.CreateTranslation(position)
                 );

            var viewProjectionMatrix = _renderingEngine.ViewProjectionMatrix;
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
                    foreach (var disposable in _bindables.OfType<IDisposable>())
                    {
                        disposable.Dispose();
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
    }
}