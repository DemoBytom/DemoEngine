// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Vortice.Direct3D12;
using static Demo.Engine.Platform.DirectX12.RenderingResources.DescriptorHeapAllocator;

namespace Demo.Engine.Platform.DirectX12.RenderingResources;

internal class RenderTexture
    : IDisposable
{
    private bool _disposedValue;
    private readonly Texture _texture;
    private readonly DescriptorHandle[] _rtv = new DescriptorHandle[Common.MAX_MIPS];
    private ushort _mipCount;

    public DescriptorHandle SRV => _texture.SRV;
    public ID3D12Resource Resource => _texture.Resource;

    private RenderTexture(
        ID3D12RenderingEngine renderingEngine,
        Texture texture,
        ResourceDescription resourceDescription)
    {
        _texture = texture;
        _mipCount = resourceDescription.MipLevels;

        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            _mipCount, Common.MAX_MIPS);

        var rtvHeap = renderingEngine.RTVHeapAllocator;

        var rtvDesc = new RenderTargetViewDescription
        {
            Format = resourceDescription.Format,
            ViewDimension = RenderTargetViewDimension.Texture2D,
            Texture2D =
            {
                MipSlice = 0,
            }
        };

        for (ushort i = 0; i < _mipCount; ++i)
        {
            _rtv[i] = rtvHeap.Allocate();
            if (!_rtv[i].IsValid)
            {
                throw new InvalidOperationException($"Invalid RTV for mip level {i}!");
            }

            renderingEngine.Device.CreateRenderTargetView(
                Resource,
                rtvDesc,
                _rtv[i].CPU!.Value);

            ++rtvDesc.Texture2D.MipSlice;
        }
    }

    /// <summary>
    /// Creates Commited Render Texture resource
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static RenderTexture CreateComittedResource(
        ID3D12RenderingEngine renderingEngine,
        ResourceDescription resourceDescription,
        ResourceStates initialStates,
        ClearValue? clearValue = null,
        ShaderResourceViewDescription? shaderResourceViewDescription = null)
        => new(
            renderingEngine: renderingEngine,
            texture: Texture.CreateComittedResource(
                renderingEngine: renderingEngine,
                resourceDescription: resourceDescription,
                initialStates: initialStates,
                clearValue: clearValue,
                shaderResourceViewDescription: shaderResourceViewDescription),
            resourceDescription: resourceDescription);

    /// <summary>
    /// Creates Placed Render Texture resource
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static RenderTexture CreatePlacedResource(
        ID3D12RenderingEngine renderingEngine,
        ID3D12Heap1 heap,
        ResourceAllocationInfo1 resourceAllocationInfo,
        ResourceDescription resourceDescription,
        ResourceStates initialStates,
        ClearValue? clearValue = null,
        ShaderResourceViewDescription? shaderResourceViewDescription = null)
        => new(
            renderingEngine: renderingEngine,
            texture: Texture.CreatePlacedResource(
                renderingEngine: renderingEngine,
                heap: heap,
                resourceAllocationInfo: resourceAllocationInfo,
                resourceDescription: resourceDescription,
                initialStates: initialStates,
                clearValue: clearValue,
                shaderResourceViewDescription: shaderResourceViewDescription),
            resourceDescription: resourceDescription);

    /// <summary>
    /// Creates Render Texture resource
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static RenderTexture CreateTexture(
       ID3D12RenderingEngine renderingEngine,
       ID3D12Resource resource,
       ShaderResourceViewDescription? shaderResourceViewDescription = null)
        => new(
            renderingEngine: renderingEngine,
            texture: Texture.CreateTexture(renderingEngine, resource, shaderResourceViewDescription),
            resourceDescription: resource.Description);

    public CpuDescriptorHandle RTV(ushort mipIndex)
        => _rtv[mipIndex] is { IsValid: true } rtv
        ? rtv.CPU.Value
        : throw new InvalidOperationException($"Invalid descriptor handle[{mipIndex}]");

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                foreach (var rtv in _rtv)
                {
                    rtv.Dispose();
                }
                _texture.Dispose();

                _mipCount = 0;
            }

            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~RenderTexture()
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