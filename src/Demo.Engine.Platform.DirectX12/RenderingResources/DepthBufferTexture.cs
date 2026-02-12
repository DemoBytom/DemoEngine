// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Vortice.Direct3D12;
using Vortice.DXGI;

namespace Demo.Engine.Platform.DirectX12.RenderingResources;

internal sealed class DepthBufferTexture
    : IDisposable
{
    private bool _disposedValue;
    private readonly Texture _texture;
    private readonly DescriptorHandle<DSVDescriptorHeapAllocator> _dsv;

    public CpuDescriptorHandle DSV
        => _dsv is { IsValid: true } dsv
        ? dsv.CPU.Value
        : throw new InvalidOperationException("Invalid descriptor handle");

    public DescriptorHandle<SRVDescriptorHeapAllocator> SRV => _texture.SRV;
    public ID3D12Resource Resource => _texture.Resource;

    private DepthBufferTexture(
        ID3D12RenderingEngine renderingEngine,
        Texture texture,
        DepthStencilViewDescription dsvDescription)
    {
        _texture = texture;
        _dsv = renderingEngine.DSVHeapAllocator.Allocate();
        renderingEngine.Device.CreateDepthStencilView(
            Resource,
            dsvDescription,
            DSV);
    }

    /// <summary>
    /// Creates Commited DepthBuffer Texture resource
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static DepthBufferTexture CreateComittedResource(
        ID3D12RenderingEngine renderingEngine,
        ResourceDescription resourceDescription,
        ResourceStates initialStates,
        ClearValue? clearValue = null)
    {
        var (srvDescription, dsvDescription) = CreateDescriptions(ref resourceDescription);

        return new DepthBufferTexture(
            renderingEngine: renderingEngine,
            texture: Texture.CreateComittedResource(
                renderingEngine: renderingEngine,
                resourceDescription: resourceDescription,
                initialStates: initialStates,
                clearValue: clearValue,
                shaderResourceViewDescription: srvDescription),
            dsvDescription: dsvDescription);
    }

    /// <summary>
    /// Creates Placed DepthBuffer Texture resource
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static DepthBufferTexture CreatePlacedResource(
        ID3D12RenderingEngine renderingEngine,
        ID3D12Heap1 heap,
        ResourceAllocationInfo1 resourceAllocationInfo,
        ResourceDescription resourceDescription,
        ResourceStates initialStates,
        ClearValue? clearValue = null)
    {
        var (srvDescription, dsvDescription) = CreateDescriptions(ref resourceDescription);

        return new DepthBufferTexture(
            renderingEngine: renderingEngine,
            texture: Texture.CreatePlacedResource(
                renderingEngine: renderingEngine,
                heap: heap,
                resourceAllocationInfo: resourceAllocationInfo,
                resourceDescription: resourceDescription,
                initialStates: initialStates,
                clearValue: clearValue,
                shaderResourceViewDescription: srvDescription),
            dsvDescription: dsvDescription);
    }

    private static (ShaderResourceViewDescription srvDesc, DepthStencilViewDescription dsvDesc) CreateDescriptions(
        ref ResourceDescription resourceDescription)
    {
        var dsvFormat = resourceDescription.Format;
        var srvDescription = new ShaderResourceViewDescription();

        if (resourceDescription.Format == Format.D32_Float)
        {
            resourceDescription.Format = Format.R32_Typeless;
            srvDescription.Format = Format.R32_Float;
        }

        //D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING
        srvDescription.Shader4ComponentMapping = ShaderComponentMapping.Default;
        srvDescription.ViewDimension = ShaderResourceViewDimension.Texture2D;
        srvDescription.Texture2D = new Texture2DShaderResourceView
        {
            MipLevels = 1,
            MostDetailedMip = 0,
            PlaneSlice = 0,
            ResourceMinLODClamp = 0.0f,
        };

        var dsvDescription = new DepthStencilViewDescription
        {
            ViewDimension = DepthStencilViewDimension.Texture2D,
            Flags = DepthStencilViewFlags.None,
            Format = dsvFormat,
            Texture2D = new Texture2DDepthStencilView
            {
                MipSlice = 0,
            },
        };

        return (srvDescription, dsvDescription);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _dsv.Dispose();
                _texture.Dispose();
            }

            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~TextBufferTexture()
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