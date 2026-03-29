// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
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
            srcData: (uint)gPassService.MainBuffer.SRV.Index,
            destOffsetIn32BitValues: 0);
        commandList.SetGraphicsRootDescriptorTable(
            rootParameterIndex: (int)RootParametersIndices.DescriptorTable,
            //                                                      TODO NRT!
            baseDescriptor: renderingEngine.SRVHeapAllocator.GPU_Start!.Value);
        commandList.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        commandList.OMSetRenderTargets(1, targetRTV);
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

        var parameters = new RootParameter1[(int)RootParametersIndices.Count];

        parameters[(int)RootParametersIndices.RootConstants] = RootParameter1
            .ConstantsRootParameter(
                numConstants: 1,
                visibility: ShaderVisibility.Pixel,
                shaderRegister: 1);
        parameters[(int)RootParametersIndices.DescriptorTable] = RootParameter1
            .DescriptorTableRootParameter(
                visibility: ShaderVisibility.Pixel,
                range);

        //const RootSignatureFlags ROOT_SIGNATURE_FLAGS =
        //   RootSignatureFlags.AllowInputAssemblerInputLayout
        //   | RootSignatureFlags.DenyAmplificationShaderRootAccess
        //   | RootSignatureFlags.DenyDomainShaderRootAccess
        //   | RootSignatureFlags.DenyGeometryShaderRootAccess
        //   | RootSignatureFlags.DenyHullShaderRootAccess
        //   | RootSignatureFlags.DenyMeshShaderRootAccess
        //   ;

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

        PipelineStateStream stream = new()
        {

            RootSignature = new(_rootSignature),
            VertexShader = new(vertexShader.ShaderBlob.Span),
            PixelShader = new(pixelShader.ShaderBlob.Span),
            PrimitiveTopology = new(primitiveTopology),
            RenderTargetFormats = new(renderTagetFormats),
            Rasterizer = new(RasterizerDescription.CullNone), // no culling
        };

        _pipelineStateObject = renderingEngine.Device.CreatePipelineState(stream);
        _pipelineStateObject.NameObject(
            "Post Process FX pipeline state object",
            logger);

        return true;
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct PipelineStateStream
    {
        public required PipelineStateSubObjectTypeRootSignature RootSignature { get; init; }
        public required PipelineStateSubObjectTypeVertexShader VertexShader { get; init; }
        public required PipelineStateSubObjectTypePixelShader PixelShader { get; init; }
        public required PipelineStateSubObjectTypePrimitiveTopology PrimitiveTopology { get; init; }
        public required PipelineStateSubObjectTypeRenderTargetFormats RenderTargetFormats { get; init; }
        public required PipelineStateSubObjectTypeRasterizer Rasterizer { get; init; }
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

    private enum RootParametersIndices
    {
        RootConstants,
        DescriptorTable,

        // MUST BE LAST! :)
        Count,
    }
}