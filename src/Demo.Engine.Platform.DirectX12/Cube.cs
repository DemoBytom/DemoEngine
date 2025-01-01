// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Platform.DirectX12.Buffers;
using Demo.Engine.Platform.DirectX12.Shaders;
using SharpGen.Runtime;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX12;

public class Cube
    : ICube,
      IDisposable
{
    private readonly IDebugLayerLogger _debugLayerLogger;
    private readonly ID3D12RenderingEngine _renderingEngine;
    private readonly CompiledVS _compiledVS;
    private readonly CompiledPS _compiledPS;

    private readonly Vertex[] _cubeVertices = new Vertex[]
    {
        new( new (-1.0f, -1.0f, -1.0f), new ( 255, 000, 000, 255)  ),
        new( new ( 1.0f, -1.0f, -1.0f), new ( 125, 125, 000, 255)  ),
        new( new (-1.0f,  1.0f, -1.0f), new ( 000, 125, 125, 255)  ),
        new( new ( 1.0f,  1.0f, -1.0f), new ( 000, 000, 255, 255)  ),
        new( new (-1.0f, -1.0f,  1.0f), new ( 125, 000, 125, 255)  ),
        new( new ( 1.0f, -1.0f,  1.0f), new ( 000, 255, 000, 255)  ),
        new( new (-1.0f,  1.0f,  1.0f), new ( 000, 000, 000, 255)  ),
        new( new ( 1.0f,  1.0f,  1.0f), new ( 000, 000, 255, 255)  ),
    };

    private readonly ushort[] _indices =
    [
            //Cube
            4,5,6, 6,5,7,
            5,1,7, 7,1,3,
            1,0,3, 3,0,2,
            0,4,2, 2,4,6,
            6,7,2, 2,7,3,
            5,4,1, 1,4,0
    ];

    private readonly CubeFacesColors _colors = new(
        Face1: new Color4(255, 000, 000, 255),
        Face2: new Color4(000, 255, 000, 255),
        Face3: new Color4(000, 000, 255, 255),
        Face4: new Color4(000, 125, 125, 255),
        Face5: new Color4(125, 125, 000, 255),
        Face6: new Color4(125, 000, 125, 255));
    private readonly VertexBuffer<Vertex> _vertexBuffer;

    private readonly ID3D12Resource _indexBuffer;
    private readonly ID3D12Resource _vsConstantBuffer;
    private readonly IndexBufferView _indexBufferView;
    private bool _disposedValue;
    private ID3D12RootSignature? _rootSignature;

    private MatricesBuffer _matricesBuffer = new(
        modelToWorldMatrix: Matrix4x4.Identity,
        viewProjectionMatrix: Matrix4x4.Identity);

    private readonly ID3D12PipelineState _pipelineState;
    private readonly ID3D12Resource _psConstantBuffer;

    public Cube(
        IDebugLayerLogger debugLayerLogger,
        ID3D12RenderingEngine renderingEngine,
        CompiledVS compiledVS,
        CompiledPS compiledPS)
    {
        _debugLayerLogger = debugLayerLogger;
        _renderingEngine = renderingEngine;

        _compiledVS = compiledVS;
        _compiledPS = compiledPS;

        _vertexBuffer = new VertexBuffer<Vertex>(
            _renderingEngine,
            _cubeVertices);

        _indexBuffer = _renderingEngine.Device.CreateCommittedResource(
            heapType: HeapType.Upload,
            HeapFlags.None,
            description: new ResourceDescription(
                dimension: ResourceDimension.Buffer,
                alignment: D3D12.DefaultResourcePlacementAlignment,
                width: sizeof(ushort) * (ulong)_indices.Length,
                height: 1,
                depthOrArraySize: 1,
                mipLevels: 1,
                format: Format.Unknown,
                sampleCount: 1,
                sampleQuality: 0,
                layout: TextureLayout.RowMajor,
                flags: ResourceFlags.None),
            initialResourceState: ResourceStates.GenericRead,
            optimizedClearValue: null);

        _vsConstantBuffer = _renderingEngine.Device.CreateCommittedResource(
            HeapType.Upload,
            ResourceDescription.Buffer((ulong)MatricesBuffer.SizeInBytes),
            ResourceStates.GenericRead);

        _indexBufferView = new IndexBufferView(
            bufferLocation: _indexBuffer.GPUVirtualAddress,
            sizeInBytes: sizeof(short) * (uint)_indices.Length,
            false);

        var indicesUpload = _indexBuffer.Map<ushort>(
            0,
            _indices.Length);
        _indices.AsSpan().CopyTo(indicesUpload);
        _indexBuffer.Unmap(0);

        _psConstantBuffer = _renderingEngine.Device.CreateCommittedResource(
            HeapType.Upload,
            ResourceDescription.Buffer(CubeFacesColors.SizeInBytes),
            ResourceStates.GenericRead);

        var gfxPipelineStateDesc = PrepareRenderingPipeline();

        _pipelineState = _renderingEngine.Device.CreateGraphicsPipelineState(gfxPipelineStateDesc);
    }

    public GraphicsPipelineStateDescription PrepareRenderingPipeline()
    {
        const RootSignatureFlags ROOT_SIGNATURE_FLAGS =
            RootSignatureFlags.AllowInputAssemblerInputLayout
            | RootSignatureFlags.DenyAmplificationShaderRootAccess
            | RootSignatureFlags.DenyDomainShaderRootAccess
            | RootSignatureFlags.DenyGeometryShaderRootAccess
            | RootSignatureFlags.DenyHullShaderRootAccess
            | RootSignatureFlags.DenyMeshShaderRootAccess
            ;

        var vertexShaderRootDescriptor = new RootDescriptor1(
            shaderRegister: 0,
            registerSpace: 0,
            flags: RootDescriptorFlags.DataStaticWhileSetAtExecute);

        var pixelShaderRootDescriptor = new RootDescriptor1(
            shaderRegister: 1,
            registerSpace: 0,
            flags: RootDescriptorFlags.DataStaticWhileSetAtExecute);

        _rootSignature = _renderingEngine.Device.CreateRootSignature(
            new RootSignatureDescription1(
                flags: ROOT_SIGNATURE_FLAGS,
                parameters: [
                    new (
                        parameterType: RootParameterType.ConstantBufferView,
                        rootDescriptor: vertexShaderRootDescriptor,
                        visibility: ShaderVisibility.Vertex),
                    new(
                        parameterType: RootParameterType.ConstantBufferView,
                        rootDescriptor: pixelShaderRootDescriptor,
                        visibility: ShaderVisibility.Pixel),
                    ])
        );

        var vertexLayout = VertexLayout();

        var gpsd = new
        GraphicsPipelineStateDescription()
        {
            RootSignature = _rootSignature,
            InputLayout = new InputLayoutDescription
            {
                Elements = vertexLayout,
            },
            IndexBufferStripCutValue = IndexBufferStripCutValue.Disabled,
            VertexShader = _compiledVS.CompiledShader,
            PixelShader = _compiledPS.CompiledShader,
            DomainShader = null,
            HullShader = null,
            GeometryShader = null,
            // Rasterizer
            PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
            RasterizerState = new RasterizerDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Back,
                FrontCounterClockwise = true,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                SlopeScaledDepthBias = 0.0f,
                DepthClipEnable = false,
                MultisampleEnable = false,
                AntialiasedLineEnable = false,
                ForcedSampleCount = 0,
            },
            StreamOutput = new StreamOutputDescription
            {
                //nulls
            },
            RenderTargetFormats =
            [
                Format.R8G8B8A8_UNorm
            ],
            DepthStencilFormat = Format.Unknown,
            BlendState = BlendDescription.Opaque,
            //BlendDescription.Opaque,
            DepthStencilState = new DepthStencilDescription
            {
                DepthEnable = false,
                DepthFunc = ComparisonFunction.Always,
                DepthWriteMask = DepthWriteMask.Zero,
                StencilEnable = false,
                StencilReadMask = 0,
                StencilWriteMask = 0,
                FrontFace = new DepthStencilOperationDescription
                {
                    StencilFunc = ComparisonFunction.Always,
                    StencilDepthFailOp = StencilOperation.Keep,
                    StencilFailOp = StencilOperation.Keep,
                    StencilPassOp = StencilOperation.Keep,
                },
                BackFace = new DepthStencilOperationDescription
                {
                    StencilFunc = ComparisonFunction.Always,
                    StencilDepthFailOp = StencilOperation.Keep,
                    StencilFailOp = StencilOperation.Keep,
                    StencilPassOp = StencilOperation.Keep,
                },
            },
            SampleMask = 0xFFFFFFFF,
            SampleDescription = new SampleDescription
            {
                Count = 1,
                Quality = 0,
            },
            NodeMask = 0,
            CachedPSO = new CachedPipelineState
            {
                CachedBlobSizeInBytes = 0,
                CachedBlob = (nint)null,
            },
            Flags = PipelineStateFlags.None,
        };

        return gpsd;
    }

    public unsafe void Draw()
    {
        // Update VS constant buffer
        var updateVS = _vsConstantBuffer.Map<MatricesBuffer>(
            0,
            1);

        fixed (void* dataPtr = &_matricesBuffer)
        {
            Unsafe.CopyBlock(
                updateVS.GetPointerUnsafe(),
                dataPtr,
                (uint)MatricesBuffer.SizeInBytes);
        }
        _vsConstantBuffer.Unmap(0);

        //Update PS constant buffer
        var updatePS = _psConstantBuffer.Map<CubeFacesColors>(
            0,
            1);

        fixed (void* dataPtr = &_colors)
        {
            Unsafe.CopyBlock(
                updatePS.GetPointerUnsafe(),
                dataPtr,
                CubeFacesColors.SizeInBytes);
        }
        _psConstantBuffer.Unmap(0);

        // PSO
        _renderingEngine.CommandList.SetPipelineState(_pipelineState);
        _renderingEngine.CommandList.SetGraphicsRootSignature(_rootSignature);

        //constant buffers
        _renderingEngine.CommandList.SetGraphicsRootConstantBufferView(
            0,
            _vsConstantBuffer.GPUVirtualAddress);

        _renderingEngine.CommandList.SetGraphicsRootConstantBufferView(
            1,
            _psConstantBuffer.GPUVirtualAddress);

        // Input Assembler
        _vertexBuffer.Bind();

        _renderingEngine.CommandList.IASetIndexBuffer(
            _indexBufferView);

        _renderingEngine.CommandList.IASetPrimitiveTopology(
            PrimitiveTopology.TriangleList);
        // Rasterizer
        var drawingArea = _renderingEngine.Control.DrawingArea;
        _renderingEngine.CommandList.RSSetViewports(new Viewport(
            x: drawingArea.X,
            y: drawingArea.Y,
            width: drawingArea.Width,
            height: drawingArea.Height,
            minDepth: 1.0f,
            maxDepth: 0.0f));

        _renderingEngine.CommandList.RSSetScissorRect(
            width: _renderingEngine.Control.DrawWidth.Value,
            height: _renderingEngine.Control.DrawHeight.Value);

        _renderingEngine.CommandList.DrawIndexedInstanced(
            indexCountPerInstance: (uint)_indices.Length,
            instanceCount: 1,
            startIndexLocation: 0,
            baseVertexLocation: 0,
            startInstanceLocation: 0);
    }

    public void Update(
        Vector3 position,
        float rotationAngleInRadians)
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

    private static InputElementDescription[] VertexLayout()
    {
        var vertexLayout = new InputElementDescription[] {
            new (
                semanticName: "position",
                semanticIndex: 0,
                format: Format.R32G32B32_Float,
                offset: 0,
                slot: 0,
                slotClass: InputClassification.PerVertexData,
                stepRate: 0),
            new (
                semanticName: "color",
                semanticIndex: 0,
                format: Format.R8G8B8A8_UNorm,
                //offset: Vertex.PositionSizeInBytes,
                offset: D3D12.AppendAlignedElement, //should automatically calculate the offset based on previous element
                slot: 0,
                slotClass: InputClassification.PerVertexData,
                stepRate: 0)
        };

        return vertexLayout;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _pipelineState.Dispose();
                _rootSignature?.Dispose();
                _vsConstantBuffer.Dispose();
                _psConstantBuffer.Dispose();
                _vertexBuffer.Dispose();
                _indexBuffer.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Cube()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}