// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Platform.DirectX.Bindable;
using Demo.Engine.Platform.DirectX.Bindable.Buffers;
using Demo.Engine.Platform.DirectX.Bindable.Shaders;
using Demo.Engine.Platform.DirectX.Interfaces;
using Demo.Engine.Platform.DirectX.Shaders;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX.Models
{
    public class Cube : DrawableBase<Cube>, ICube
    {
        private MatricesBuffer _matricesBuffer = new(Matrix4x4.Identity, Matrix4x4.Identity);

        private readonly Vertex[] _cubeVertices = new Vertex[]
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
            CompiledVS compiledVS,
            CompiledPS compiledPS) :
            base(
                renderingEngine,
                t => t.PrepareBindables(compiledVS, compiledPS))
        {
        }

        private IBindable[] PrepareBindables(
            CompiledVS compiledVS,
            CompiledPS compiledPS)
        {
            var vertexShader = new VertexShader(
                //"Shaders/Triangle/TriangleVS.hlsl",
                //shaderCompiler,
                compiledVS,
                _renderingEngine);
            var pixelShader = new PixelShader(
                //"Shaders/Triangle/TrianglePS.hlsl",
                //shaderCompiler,
                compiledPS,
                _renderingEngine);

            //VertexBuffer
            var vertexBuffer = new VertexBuffer<Vertex>(
                _renderingEngine,
                _cubeVertices,
                Vertex.SizeInBytes);

            //IndexBuffer
            var indexBuffer = new IndexBuffer<ushort>(
                _renderingEngine,
                in _triangleIndices,
                sizeof(ushort));
            var inputLayout = new InputLayout(
                _renderingEngine,
                new[]
                {
                    new InputElementDescription(
                        semanticName: "position",
                        semanticIndex: 0,
                        format: Format.R32G32B32_Float,
                        offset: 0,
                        slot: 0,
                        slotClass: InputClassification.PerVertexData,
                        stepRate: 0),
                    new InputElementDescription(
                        semanticName: "color",
                        semanticIndex: 0,
                        format: Format.R8G8B8A8_UNorm,
                        offset: Vertex.PositionSizeInBytes,
                        slot: 0,
                        slotClass: InputClassification.PerVertexData,
                        stepRate: 0)
                }, vertexShader.CompiledShader);

            var colors = new CubeFacesColors(
                new Color4(255, 000, 000, 255),
                new Color4(000, 255, 000, 255),
                new Color4(000, 000, 255, 255),
                new Color4(000, 125, 125, 255),
                new Color4(125, 125, 000, 255),
                new Color4(125, 000, 125, 255));
            var colorsConstantBuffer = new PSConstantBuffer<CubeFacesColors>(
                _renderingEngine,
                ref colors);

            var matricesConstantBuffer = new VSConstantBuffer<MatricesBuffer>(
                _renderingEngine,
                ref _matricesBuffer);

            return new IBindable[] {
                matricesConstantBuffer,
                colorsConstantBuffer,
                vertexShader,
                pixelShader,
                vertexBuffer,
                indexBuffer,
                inputLayout,
                new Topology(
                    _renderingEngine,
                    PrimitiveTopology.TriangleList) };
        }

        protected override void UpdateUpdatables()
        {
            var matricesBuffer = GetUpdatable<VSConstantBuffer<MatricesBuffer>>();
            matricesBuffer.Update(in _matricesBuffer);
        }

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
        }
    }
}