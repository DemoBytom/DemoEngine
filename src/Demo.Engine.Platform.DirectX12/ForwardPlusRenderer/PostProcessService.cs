// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using Demo.Engine.Platform.DirectX12.Shaders;
using Microsoft.Extensions.Logging;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace Demo.Engine.Platform.DirectX12.ForwardPlusRenderer;

internal sealed class PostProcessService(
    ILogger<PostProcessService> logger,
    ID3D12RenderingEngine renderingEngine,
    IGPassService gPassService,
    IEngineShaderManager engineShaderManager)
    : IPostProcessService,
      IDisposable
{
    private bool _disposedValue;

    private ID3D12RootSignature? _rootSignature = null;
    private ID3D12PipelineState? _pipelineStateObject = null;

    [MemberNotNullWhen(true,
        nameof(_rootSignature),
        nameof(_pipelineStateObject))]
    public bool Initialize()
        => CreateFxPsoAndRootSignature()
        ;

    /// <summary>
    /// Post process method
    /// </summary>
    /// <param name="commandList">command list to execute on</param>
    /// <param name="targetRTV">target render target view</param>
    public void PostProcess(
        ID3D12GraphicsCommandList commandList,
        CpuDescriptorHandle targetRTV)
    {
        commandList.SetGraphicsRootSignature(_rootSignature);
        commandList.SetPipelineState(_pipelineStateObject);

        commandList.SetGraphicsRoot32BitConstant(
            rootParameterIndex: (int)RootParametersIndices.RootConstants,
            srcData: gPassService.MainBuffer.SRV.Index,
            destOffsetIn32BitValues: 0);
        commandList.SetGraphicsRootDescriptorTable(
            rootParameterIndex: (int)RootParametersIndices.DescriptorTable,
            //                                                      TODO NRT!
            baseDescriptor: renderingEngine.SRVHeapAllocator.GPU_Start!.Value);
        commandList.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        commandList.OMSetRenderTargets(targetRTV);
        commandList.DrawInstanced(
            vertexCountPerInstance: 3,
            instanceCount: 1,
            startVertexLocation: 0,
            startInstanceLocation: 0);
    }

    [MemberNotNullWhen(true,
        nameof(_rootSignature),
        nameof(_pipelineStateObject))]
    private bool CreateFxPsoAndRootSignature()
    {
        if (_rootSignature is not null)
        {
            throw new InvalidOperationException("Root signature already created!");
        }
        if (_pipelineStateObject is not null)
        {
            throw new InvalidOperationException("Pipeline state object already created!");
        }

        var range = new DescriptorRange1(
            rangeType: DescriptorRangeType.ShaderResourceView,
            numDescriptors: D3D12.DescriptorRangeOffsetAppend,
            baseShaderRegister: 0,
            registerSpace: 0,
            flags: DescriptorRangeFlags.DescriptorsVolatile);

        RootParameter1[] parameters =
        [
            RootParameter1.ConstantBufferViewRootParameter(
                visibility: ShaderVisibility.Pixel,
                shaderRegister: 1,
                registerSpace: 1,
                flags: RootDescriptorFlags.DataStaticWhileSetAtExecute),
            RootParameter1.DescriptorTableRootParameter(
                visibility: ShaderVisibility.Pixel,
                range)
        ];

        _rootSignature = renderingEngine.Device.CreateRootSignature(
            new RootSignatureDescription1(
                //ROOT_SIGNATURE_FLAGS,
                RootSignatureExtensions.DenyAll,
                parameters));

        _rootSignature.NameObject(
            "Post Process FX root signature",
            logger);

        var vertexShader = engineShaderManager.GetShader(ShaderId.FullscreenTriangleVS);
        var pixelShader = engineShaderManager.GetShader(ShaderId.PostProcessPS);
        var primitiveTopology = PrimitiveTopologyType.Triangle;
        var renderTagetFormats = new Format[] { Common.DEFAULT_BACK_BUFFER_FORMAT };

        _pipelineStateObject = renderingEngine.Device.CreateGraphicsPipelineState(
            new GraphicsPipelineStateDescription()
            {
                RootSignature = _rootSignature,
                VertexShader = vertexShader.ShaderBlob,
                PixelShader = pixelShader.ShaderBlob,
                PrimitiveTopologyType = primitiveTopology,
                RenderTargetFormats = renderTagetFormats,
                RasterizerState = RasterizerDescription.CullNone, // no culling
            });
        _pipelineStateObject.NameObject(
            "Post Process FX pipeline state object",
            logger);

        return true;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _rootSignature?.Dispose();
                _pipelineStateObject?.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    enum RootParametersIndices
    {
        RootConstants = 0,
        DescriptorTable = 1,
    }
}