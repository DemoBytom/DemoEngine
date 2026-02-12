// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Vortice.Direct3D12;

namespace Demo.Engine.Platform.DirectX12.RenderingResources;

internal sealed class Texture
    : IDisposable
{
    private bool _disposedValue;
    private readonly ID3D12RenderingEngine _renderingEngine;
    private readonly ILogger<Texture> _logger;

    public ID3D12Resource Resource { get; }

    public DescriptorHandle<SRVDescriptorHeapAllocator> SRV { get; }

    /// <summary>
    /// Creates Texture resource
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private Texture(
        ID3D12RenderingEngine renderingEngine,
        ID3D12Resource resource,
        ShaderResourceViewDescription? shaderResourceViewDescription = null)
    {
        _renderingEngine = renderingEngine;
        _logger = renderingEngine.GetRequiredService<ILogger<Texture>>();

        Resource = resource;

        SRV = _renderingEngine.SRVHeapAllocator.Allocate();
        if (!SRV.IsValid)
        {
            _logger.LogError("Invalid SRV descriptor!");
            throw new InvalidOperationException("Invalid SRV descriptor!");
        }

        _renderingEngine.Device.CreateShaderResourceView(
            resource: Resource,
            desc: shaderResourceViewDescription,
            destDescriptor: SRV.CPU.Value);
    }

    /// <summary>
    /// Creates Placed Texture resource
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private Texture(
        ID3D12RenderingEngine renderingEngine,
        ID3D12Heap1 heap,
        ResourceAllocationInfo1 resourceAllocationInfo,
        ResourceDescription resourceDescription,
        ResourceStates initialStates,
        ClearValue? clearValue = null,
        ShaderResourceViewDescription? shaderResourceViewDescription = null)
        : this(
              renderingEngine: renderingEngine,
              resource: CreatePlacedResource(renderingEngine, heap, resourceAllocationInfo, resourceDescription, initialStates, clearValue),
              shaderResourceViewDescription: shaderResourceViewDescription)
    {
    }

    /// <summary>
    /// Creates Commited Texture resource
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private Texture(
        ID3D12RenderingEngine renderingEngine,
        ResourceDescription resourceDescription,
        ResourceStates initialStates,
        ClearValue? clearValue = null,
        ShaderResourceViewDescription? shaderResourceViewDescription = null)
        : this(
             renderingEngine: renderingEngine,
             resource: CreateCommittedResource(renderingEngine, resourceDescription, initialStates, clearValue),
             shaderResourceViewDescription: shaderResourceViewDescription)
    {
    }

    /// <inheritdoc cref="Texture(ID3D12RenderingEngine, ResourceDescription, ResourceStates, ClearValue?, ShaderResourceViewDescription?)"/>
    public static Texture CreateComittedResource(
        ID3D12RenderingEngine renderingEngine,
        ResourceDescription resourceDescription,
        ResourceStates initialStates,
        ClearValue? clearValue = null,
        ShaderResourceViewDescription? shaderResourceViewDescription = null)
        => new(
            renderingEngine: renderingEngine,
            resourceDescription: resourceDescription,
            initialStates: initialStates,
            clearValue: clearValue,
            shaderResourceViewDescription: shaderResourceViewDescription);

    /// <inheritdoc cref="Texture(ID3D12RenderingEngine, ID3D12Heap1, ResourceAllocationInfo1, ResourceDescription, ResourceStates, ClearValue?, ShaderResourceViewDescription?)"/>
    public static Texture CreatePlacedResource(
        ID3D12RenderingEngine renderingEngine,
        ID3D12Heap1 heap,
        ResourceAllocationInfo1 resourceAllocationInfo,
        ResourceDescription resourceDescription,
        ResourceStates initialStates,
        ClearValue? clearValue = null,
        ShaderResourceViewDescription? shaderResourceViewDescription = null)
        => new(
            renderingEngine: renderingEngine,
            heap: heap,
            resourceAllocationInfo: resourceAllocationInfo,
            resourceDescription: resourceDescription,
            initialStates: initialStates,
            clearValue: clearValue,
            shaderResourceViewDescription: shaderResourceViewDescription);

    /// <inheritdoc cref="Texture(ID3D12RenderingEngine, ID3D12Resource, ShaderResourceViewDescription?)"/>
    public static Texture CreateTexture(
        ID3D12RenderingEngine renderingEngine,
        ID3D12Resource resource,
        ShaderResourceViewDescription? shaderResourceViewDescription = null)
        => new(
            renderingEngine: renderingEngine,
            resource: resource,
            shaderResourceViewDescription: shaderResourceViewDescription);

    private static ClearValue? GetClearValue(
        ResourceDescription? resourceDescription,
        ClearValue? clearValue)
        => resourceDescription is not null
        && (resourceDescription.Value.Flags.HasFlag(ResourceFlags.AllowRenderTarget)
            || resourceDescription.Value.Flags.HasFlag(ResourceFlags.AllowDepthStencil))
        ? clearValue
        : null;

    private static ID3D12Resource CreatePlacedResource(
        ID3D12RenderingEngine renderingEngine,
        ID3D12Heap1 heap,
        ResourceAllocationInfo1 resourceAllocationInfo,
        ResourceDescription resourceDescription,
        ResourceStates initialStates,
        ClearValue? clearValue)
    {
        var resource = renderingEngine.Device
            .CreatePlacedResource<ID3D12Resource>(
                heap: heap,
                heapOffset: resourceAllocationInfo.Offset,
                resourceDescription: resourceDescription,
                initialState: initialStates,
                clearValue: GetClearValue(resourceDescription, clearValue));

        return resource ?? ThrowFailureCreatingResource();
    }

    private static ID3D12Resource CreateCommittedResource(
        ID3D12RenderingEngine renderingEngine,
        ResourceDescription resourceDescription,
        ResourceStates initialStates,
        ClearValue? clearValue)
    {
        var resource = renderingEngine.Device
            .CreateCommittedResource(
                heapProperties: HeapProperties.DefaultHeapProperties,
                    heapFlags: HeapFlags.None,
                    description: resourceDescription,
                    initialResourceState: initialStates,
                    optimizedClearValue: GetClearValue(resourceDescription, clearValue));

        return resource ?? ThrowFailureCreatingResource();
    }

    [DoesNotReturn]
    private static ID3D12Resource ThrowFailureCreatingResource()
        => throw new InvalidOperationException("Failure creating a resource!");

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)

                SRV.Dispose();

                _renderingEngine.DeferredRelease(Resource);
            }

            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Texture()
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