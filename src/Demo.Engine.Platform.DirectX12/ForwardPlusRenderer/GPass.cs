// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using Demo.Engine.Core.ValueObjects;
using Demo.Engine.Platform.DirectX12.RenderingResources;
using Demo.Engine.Platform.DirectX12.Shaders;
using Microsoft.Extensions.Logging;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace Demo.Engine.Platform.DirectX12.ForwardPlusRenderer;

/// <summary>
/// Geometry pass - Depth (pre)pass + render pass
/// </summary>
/// <param name="logger"></param>
/// <param name="renderingEngine"></param>
internal sealed class GPass(
    ILogger<GPass> logger,
    ID3D12RenderingEngine renderingEngine,
    IEngineShaderManager engineShaderManager)
    : IGPass,
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

    public void Render(
        ID3D12GraphicsCommandList commandList,
        in FrameInfo frameInfo)
    {
        //TODO
        // Set Root Signature
        // Set Pipeline State Object
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
            RootParameter1.ConstantBufferViewRootParameter(
                visibility: ShaderVisibility.Vertex,
                shaderRegister: 0,
                registerSpace: 0,
                flags: RootDescriptorFlags.DataStaticWhileSetAtExecute),
            RootParameter1.ConstantBufferViewRootParameter(
                visibility: ShaderVisibility.Pixel,
                shaderRegister: 1,
                registerSpace: 0,
                flags: RootDescriptorFlags.DataStaticWhileSetAtExecute),
        ];

        const RootSignatureFlags ROOT_SIGNATURE_FLAGS =
            RootSignatureFlags.AllowInputAssemblerInputLayout
            | RootSignatureFlags.DenyAmplificationShaderRootAccess
            | RootSignatureFlags.DenyDomainShaderRootAccess
            | RootSignatureFlags.DenyGeometryShaderRootAccess
            | RootSignatureFlags.DenyHullShaderRootAccess
            | RootSignatureFlags.DenyMeshShaderRootAccess
            ;

        // This should be an equivalent to the above
        //const RootSignatureFlags ROOT_SIGNATURE_FLAGS = RootSignatureHelpers.DenyAll
        //    & ~RootSignatureFlags.DenyPixelShaderRootAccess
        //    & ~RootSignatureFlags.DenyVertexShaderRootAccess
        //    & RootSignatureFlags.AllowInputAssemblerInputLayout
        //    ;

        _rootSignature = renderingEngine.Device.CreateRootSignature(
            new RootSignatureDescription1(
                ROOT_SIGNATURE_FLAGS,
                parameters));

        _rootSignature.NameObject(
            "GPass root signature",
            logger);

        var vertexShader = engineShaderManager.GetShader(ShaderId.TriangleVS);
        var pixelShader = engineShaderManager.GetShader(ShaderId.TrianglePS);
        var primitiveTopology = PrimitiveTopologyType.Triangle;
        var renderTagetFormats = new Format[] { MAIN_BUFFER_FORMAT };
        var depthStencilFormat = DEPTH_BUFFER_FORMAT;

        _pipelineStateObject = renderingEngine.Device.CreateGraphicsPipelineState(
            new GraphicsPipelineStateDescription()
            {
                RootSignature = _rootSignature,
                VertexShader = vertexShader.ShaderBlob,
                PixelShader = pixelShader.ShaderBlob,
                PrimitiveTopologyType = primitiveTopology,
                RenderTargetFormats = renderTagetFormats,
                DepthStencilFormat = depthStencilFormat,
                RasterizerState = RasterizerDescription.CullNone, // no culling
                DepthStencilState = DepthStencilDescription.None, // disabled
            });
        _pipelineStateObject.NameObject(
            "GPass pipeline state object",
            logger);

        return true;
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