// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Numerics;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Platform.DirectX12.Shaders;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.DXGI;

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

    private readonly ID3D12Resource _uploadBuffer;
    private readonly ID3D12Resource _vertexBuffer;
    private readonly VertexBufferView _vertexBufferView;
    private bool _disposedValue;
    private ID3D12RootSignature? _rootSignature;
    private readonly ID3D12PipelineState _pipelineState;

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

        var uploadBufferProperties = new HeapProperties(
            type: HeapType.Upload,
            cpuPageProperty: CpuPageProperty.Unknown,
            memoryPoolPreference: MemoryPool.Unknown,
            creationNodeMask: 0,
            visibleNodeMask: 0);

        var defaultBufferProperties = new HeapProperties(
            type: HeapType.Default,
            cpuPageProperty: CpuPageProperty.Unknown,
            memoryPoolPreference: MemoryPool.Unknown,
            creationNodeMask: 0,
            visibleNodeMask: 0);

        var resourceDescription = new ResourceDescription(
            dimension: ResourceDimension.Buffer,
            alignment: D3D12.DefaultResourcePlacementAlignment,
            width: 1024,
            height: 1,
            depthOrArraySize: 1,
            mipLevels: 1,
            format: Format.Unknown,
            sampleCount: 1,
            sampleQuality: 0,
            layout: TextureLayout.RowMajor,
            flags: ResourceFlags.None);

        _uploadBuffer = _renderingEngine.Device.CreateCommittedResource(
            heapProperties: uploadBufferProperties,
            heapFlags: HeapFlags.None,
            description: resourceDescription,
            initialResourceState: ResourceStates.Common,
            optimizedClearValue: null);

        _vertexBuffer = _renderingEngine.Device.CreateCommittedResource(
            heapProperties: defaultBufferProperties,
            heapFlags: HeapFlags.None,
            description: resourceDescription,
            initialResourceState: ResourceStates.Common,
            optimizedClearValue: null);

        //var bufferData = _uploadBuffer.Map<char>(0, 1024);
        //ReadOnlySpan<char> src = "Hello World";

        //src.CopyTo(bufferData);

        //_uploadBuffer.Unmap(0);

        _vertexBufferView = new VertexBufferView(
            bufferLocation: _vertexBuffer.GPUVirtualAddress,
            sizeInBytes: (uint)_cubeVertices.Length * Vertex.SizeInBytes,
            strideInBytes: Vertex.SizeInBytes);

        var gfxPipelineStateDesc = PrepareRenderingPipeline();

        _pipelineState = _renderingEngine.Device.CreateGraphicsPipelineState(gfxPipelineStateDesc);
    }

    public GraphicsPipelineStateDescription PrepareRenderingPipeline()
    {
        const RootSignatureFlags ROOT_SIGNATURE_FLAGS =
            RootSignatureFlags.AllowInputAssemblerInputLayout
            //| RootSignatureFlags.DenyVertexShaderRootAccess
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
                CullMode = CullMode.None,
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
            BlendState = new BlendDescription(
                Blend.Zero,
                Blend.Zero,
                Blend.Zero,
                Blend.Zero),
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

    public void Draw()
    {
        //_renderingEngine.CommandList
        //    .CopyBufferRegion(
        //        dstBuffer: _vertexBuffer,
        //        dstOffset: 0,
        //        srcBuffer: _uploadBuffer,
        //        srcOffset: 0,
        //        numBytes: 1024);
        // PSO
        _renderingEngine.CommandList.SetPipelineState(_pipelineState);
        _renderingEngine.CommandList.SetGraphicsRootSignature(_rootSignature);

        // Input Assembler
        _renderingEngine.CommandList.IASetVertexBuffers(
            slot: 0,
            vertexBufferView: _vertexBufferView);

        _renderingEngine.CommandList.IASetPrimitiveTopology(
            PrimitiveTopology.TriangleList);

        _renderingEngine.CommandList.DrawInstanced(
            vertexCountPerInstance: (uint)_cubeVertices.Length,
            instanceCount: 1,
            startVertexLocation: 0,
            startInstanceLocation: 0);
    }

    public void Update(
        Vector3 position,
        float rotationAngleInRadians)
    {
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
                _vertexBuffer.Dispose();
                _uploadBuffer.Dispose();
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