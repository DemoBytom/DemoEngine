// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Demo.Engine.Core.ValueObjects;
using Demo.Engine.Platform.DirectX12.RenderingResources;
using Demo.Engine.Platform.DirectX12.Shaders;
using Microsoft.Extensions.Logging;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace Demo.Engine.Platform.DirectX12.ForwardPlusRenderer;

/// <summary>
/// Geometry pass - Depth (pre)pass + render pass
/// </summary>
/// <param name="logger"></param>
/// <param name="renderingEngine"></param>
internal sealed class GPassService(
    ILogger<GPassService> logger,
    ID3D12RenderingEngine renderingEngine,
    IEngineShaderManager engineShaderManager)
    : IGPassService,
      IDisposable
{
    private bool _disposedValue;

    private const Format MAIN_BUFFER_FORMAT = Format.R16G16B16A16_Float;
    private const Format DEPTH_BUFFER_FORMAT = Format.D32_Float;

    private static readonly (Width width, Height height) _defaultSize = (1024, 768);

    private (Width width, Height height) _currentSize = _defaultSize;

    private RenderTexture? _gpasMainBuffer = null;
    private DepthBufferTexture? _gpassDepthBuffer = null;
    private ID3D12RootSignature? _rootSignature = null;
    private ID3D12PipelineState? _pipelineStateObject = null;

    // TODO NRT!
    public RenderTexture MainBuffer => _gpasMainBuffer!;
    public DepthBufferTexture DepthBuffer => _gpassDepthBuffer!;

#if DEBUG
    private readonly ClearValue _clearValue = new()
    {
        Format = MAIN_BUFFER_FORMAT,
        Color = new(0.5f, 0.5f, 0.5f, 1),
    };
#else
    private readonly ClearValue _clearValue = new()
    {
        Format = MAIN_BUFFER_FORMAT,
        Color = new(0.0f, 0.0f, 0.0f, 1),
    };
#endif

    [MemberNotNullWhen(true,
        nameof(_gpasMainBuffer),
        nameof(_gpassDepthBuffer),
        nameof(_rootSignature),
        nameof(_pipelineStateObject))]
    public bool Initialize()
        => CreateBuffers(_defaultSize.width, _defaultSize.height)
        && CreatePsoAndRootSignature()
        ;

    public void SetSize(Width width, Height height)
    {
        var (currentWidth, currentHeight) = _currentSize;
        if (width > currentWidth || height > currentHeight)
        {
            var (newWidth, newHeight) = (
                Width.Max(currentWidth, width),
                Height.Max(currentHeight, height));

            if (CreateBuffers(newWidth, newHeight))
            {
                _currentSize = (newWidth, newHeight);
            }
            else
            {
                logger.LogErrorCreatingGPassBuffers(width, height);
            }
        }
    }

    public void DepthPrepass(
        ID3D12GraphicsCommandList commandList,
        in FrameInfo frameInfo)
    {
        //TODO
    }

    int _frame = 0;

    private readonly struct MandlebrotData
    {
        public float Width { get; init; }
        public float Height { get; init; }
        public uint Frame { get; init; }
    }

    public void Render(
        ID3D12GraphicsCommandList commandList,
        in FrameInfo frameInfo)
    {
        commandList.SetGraphicsRootSignature(_rootSignature);
        commandList.SetPipelineState(_pipelineStateObject);

        var mandlebrotData = new MandlebrotData
        {
            Width = _currentSize.width.Value,
            Height = _currentSize.height.Value,
            Frame = (uint)++_frame,
        };
        commandList.SetGraphicsRoot32BitConstants(
            rootParameterIndex: 0,
            srcData: mandlebrotData,
            destOffsetIn32BitValues: 0);

        commandList.IASetPrimitiveTopology(
            PrimitiveTopology.TriangleList);

        commandList.DrawInstanced(
            vertexCountPerInstance: 3,
            instanceCount: 1,
            startVertexLocation: 0,
            startInstanceLocation: 0);
    }

    public void AddTransitionForDepthPrepass(ResourceBarrierGroup barriers)
        => barriers.AddTransitionBarrier(
            resource: _gpassDepthBuffer!.Resource,
            before: ResourceStates.DepthRead
                  | ResourceStates.PixelShaderResource
                  | ResourceStates.NonPixelShaderResource,
            after: ResourceStates.DepthWrite);

    public void AddTransitionForGPass(ResourceBarrierGroup barriers)
        => barriers
            .AddTransitionBarrier(
                resource: _gpasMainBuffer!.Resource,
                before: ResourceStates.PixelShaderResource,
                after: ResourceStates.RenderTarget)
            .AddTransitionBarrier(
                resource: _gpassDepthBuffer!.Resource,
                before: ResourceStates.DepthWrite,
                after: ResourceStates.DepthRead
                     | ResourceStates.PixelShaderResource
                     | ResourceStates.NonPixelShaderResource);

    public void AddTranstionForPostProcess(ResourceBarrierGroup barriers)
        => barriers.AddTransitionBarrier(
            resource: _gpasMainBuffer!.Resource,
            before: ResourceStates.RenderTarget,
            after: ResourceStates.PixelShaderResource);

    public void SetRenderTargetsForDepthPrepass(ID3D12GraphicsCommandList commandList)
    {
        commandList.ClearDepthStencilView(
            depthStencilView: _gpassDepthBuffer!.DSV,
            clearFlags: ClearFlags.Depth | ClearFlags.Stencil,
            depth: 0.0f,
            stencil: 0);
        commandList.OMSetRenderTargets(
            renderTargetDescriptor: CpuDescriptorHandle.Default,
            depthStencilDescriptor: _gpassDepthBuffer!.DSV);
    }

    public void SetRenderTargetsForGPass(ID3D12GraphicsCommandList commandList)
    {
        var rtv = _gpasMainBuffer!.RTV(0);
        var dsv = _gpassDepthBuffer!.DSV;

        commandList.ClearRenderTargetView(
            renderTargetView: rtv,
            color: _clearValue.Color);
        commandList.OMSetRenderTargets(
            renderTargetDescriptor: rtv,
            depthStencilDescriptor: dsv);
    }

    [MemberNotNullWhen(true, nameof(_gpasMainBuffer), nameof(_gpassDepthBuffer))]
    private bool CreateBuffers(Width width, Height height)
    {
        _gpasMainBuffer?.Dispose();
        _gpassDepthBuffer?.Dispose();

        //Create main buffer
        _gpasMainBuffer = RenderTexture.CreateComittedResource(
            renderingEngine: renderingEngine,
            resourceDescription: new()
            {
                Alignment = 0,
                DepthOrArraySize = 1,
                Dimension = ResourceDimension.Texture2D,
                Flags = ResourceFlags.AllowRenderTarget,
                Format = MAIN_BUFFER_FORMAT,
                Layout = TextureLayout.Unknown,
                MipLevels = 0, // automatically calculate mip levels count
                SampleDescription = new(1, 0),

                Height = (uint)height.Value,
                Width = (uint)width.Value,
            },
            initialStates: ResourceStates.PixelShaderResource,
            clearValue: _clearValue,
            shaderResourceViewDescription: null);

        //Create depth buffer
        _gpassDepthBuffer = DepthBufferTexture.CreateComittedResource(
            renderingEngine: renderingEngine,
            resourceDescription: new()
            {
                Alignment = 0,
                DepthOrArraySize = 1,
                Dimension = ResourceDimension.Texture2D,
                Flags = ResourceFlags.AllowDepthStencil,
                Format = DEPTH_BUFFER_FORMAT,
                Layout = TextureLayout.Unknown,
                MipLevels = 1,
                SampleDescription = new(1, 0),
                Height = (uint)height.Value,
                Width = (uint)width.Value,
            },
            initialStates: ResourceStates.DepthRead
                         | ResourceStates.PixelShaderResource
                         | ResourceStates.NonPixelShaderResource,
            clearValue: new()
            {
                Format = DEPTH_BUFFER_FORMAT,
                DepthStencil = new()
                {
                    Depth = 0.0f,
                    Stencil = 0,
                },
            });

        _gpasMainBuffer.Resource.NameObject(
            "GPass main buffer",
            logger);
        _gpassDepthBuffer.Resource.NameObject(
            "GPass depth buffer",
            logger);

        return _gpasMainBuffer.Resource is not null
            && _gpassDepthBuffer.Resource is not null;
    }

    [MemberNotNullWhen(true, nameof(_rootSignature), nameof(_pipelineStateObject))]
    private bool CreatePsoAndRootSignature()
    {
        if (_rootSignature is not null)
        {
            throw new InvalidOperationException("Root signature already created!");
        }
        if (_pipelineStateObject is not null)
        {
            throw new InvalidOperationException("Pipeline state object already created!");
        }

        RootParameter1[] parameters =
        [
            //RootParameter1.ConstantBufferViewRootParameter(
            //    visibility: ShaderVisibility.Vertex,
            //    shaderRegister: 0,
            //    registerSpace: 0,
            //    flags: RootDescriptorFlags.DataStaticWhileSetAtExecute),
            RootParameter1.ConstantsRootParameter(
                numConstants: 3,
                visibility: ShaderVisibility.Pixel,
                shaderRegister: 1),
        ];

        //const RootSignatureFlags ROOT_SIGNATURE_FLAGS =
        //    RootSignatureFlags.AllowInputAssemblerInputLayout
        //    | RootSignatureFlags.DenyAmplificationShaderRootAccess
        //    | RootSignatureFlags.DenyDomainShaderRootAccess
        //    | RootSignatureFlags.DenyGeometryShaderRootAccess
        //    | RootSignatureFlags.DenyHullShaderRootAccess
        //    | RootSignatureFlags.DenyMeshShaderRootAccess
        //    ;

        // This should be an equivalent to the above
        //const RootSignatureFlags ROOT_SIGNATURE_FLAGS = RootSignatureHelpers.DenyAll
        //    & ~RootSignatureFlags.DenyPixelShaderRootAccess
        //    & ~RootSignatureFlags.DenyVertexShaderRootAccess
        //    & RootSignatureFlags.AllowInputAssemblerInputLayout
        //    ;

        _rootSignature = renderingEngine.Device.CreateRootSignature(
            new RootSignatureDescription1(
                //ROOT_SIGNATURE_FLAGS,
                RootSignatureExtensions.DenyAll
                & ~RootSignatureFlags.DenyPixelShaderRootAccess,
                parameters));

        _rootSignature.NameObject(
            "GPass root signature",
            logger);

        //var vertexShader = engineShaderManager.GetShader(ShaderId.TriangleVS);
        //var pixelShader = engineShaderManager.GetShader(ShaderId.TrianglePS);
        var vertexShader = engineShaderManager.GetShader(ShaderId.FullscreenTriangleVS);
        var pixelShader = engineShaderManager.GetShader(ShaderId.FillColorPS);
        var primitiveTopology = PrimitiveTopologyType.Triangle;
        var renderTagetFormats = new Format[] { MAIN_BUFFER_FORMAT };
        var depthStencilFormat = DEPTH_BUFFER_FORMAT;

        //        // TEMP - vertex layout from Cube
        //        //InputLayout = new InputLayoutDescription
        //        //{
        //        //    Elements = Cube.VertexLayout(),
        //        //}

        PipelineStateStream stream = new()
        {
            RootSignature = new(_rootSignature),
            VertexShader = new(vertexShader.ShaderBlob.Span),
            PixelShader = new(pixelShader.ShaderBlob.Span),
            PrimitiveTopology = new(primitiveTopology),
            RenderTargetFormats = new(renderTagetFormats),
            DepthStencilFormat = new(depthStencilFormat),
            Rasterizer = new(RasterizerDescription.CullNone),
            DepthStencil = new(DepthStencilDescription.None),
        };

        _pipelineStateObject = renderingEngine.Device.CreatePipelineState(stream);

        _pipelineStateObject.NameObject(
            "GPass pipeline state object",
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
        public required PipelineStateSubObjectTypeDepthStencilFormat DepthStencilFormat { get; init; }
        public required PipelineStateSubObjectTypeRasterizer Rasterizer { get; init; }
        public required PipelineStateSubObjectTypeDepthStencil DepthStencil { get; init; }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _gpassDepthBuffer?.Dispose();
                _gpasMainBuffer?.Dispose();
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
}