// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using Demo.Engine.Core.ValueObjects;
using Demo.Engine.Platform.DirectX12.RenderingResources;
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
    ID3D12RenderingEngine renderingEngine)
    : IDisposable
{
    private bool _disposedValue;

    private const Format MAIN_BUFFER_FORMAT = Format.R16G16B16A16_Float;
    private const Format DEPTH_BUFFER_FORMAT = Format.D32_Float;

    private static readonly (Width width, Height height) _defaultSize = (1024, 768);

    private (Width width, Height height) _currentSize = _defaultSize;

    private RenderTexture? _gpasMainBuffer = null;
    private DepthBufferTexture? _gpassDepthBuffer = null;

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

    public bool Initialize()
        => CreateBuffers(_defaultSize.width, _defaultSize.height);

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
#pragma warning disable CA1873 // Avoid potentially expensive logging
                logger.LogError("Failed to create GPass buffers with size {Width}x{Height}", width, height);
#pragma warning restore CA1873 // Avoid potentially expensive logging
            }
        }
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
                MipLevels = 0, //make space for all mip levels
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
    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _gpassDepthBuffer?.Dispose();
                _gpasMainBuffer?.Dispose();
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