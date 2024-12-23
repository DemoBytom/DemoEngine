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

        _ = PrepareRenderingPipeline();
    }

    public GraphicsPipelineStateDescription PrepareRenderingPipeline()
    {
        var rootSignatureFlags =
            RootSignatureFlags.AllowInputAssemblerInputLayout
            | RootSignatureFlags.DenyVertexShaderRootAccess
            ;

        RootDescriptor1 rootDescriptor1 = new(
            shaderRegister: 0,
            registerSpace: 0,
            flags: RootDescriptorFlags.DataStaticWhileSetAtExecute);

        _rootSignature = _renderingEngine.Device.CreateRootSignature(
            new RootSignatureDescription1(
                flags: rootSignatureFlags,
                parameters: [
                    new RootParameter1(
                        parameterType: RootParameterType.ConstantBufferView,
                        rootDescriptor: rootDescriptor1,
                        visibility: ShaderVisibility.Vertex)
                    ])
        );

        var vertexData = VertexData();

        var gpsd = new
        GraphicsPipelineStateDescription()
        {
            RootSignature = _rootSignature,
            InputLayout = new InputLayoutDescription
            {
                Elements = vertexData,
            },
            IndexBufferStripCutValue = IndexBufferStripCutValue.Disabled,
            VertexShader = _compiledVS.CompiledShader,
            // TODO: Rasterizer
            PixelShader = _compiledPS.CompiledShader,
            // TODO: Output Merger
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

    private static InputElementDescription[] VertexData()
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
                _vertexBuffer.Dispose();
                _uploadBuffer.Dispose();
                _rootSignature?.Dispose();
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