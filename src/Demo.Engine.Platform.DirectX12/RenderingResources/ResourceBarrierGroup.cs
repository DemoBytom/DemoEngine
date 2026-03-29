// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Vortice.Direct3D12;

namespace Demo.Engine.Platform.DirectX12.RenderingResources;

/// <summary>
/// A reusable resource barrier group, that can be used to batch resource barriers together,
/// and avoid allocating new arrays for them every frame.
/// </summary>
/// <param name="size">The maximum number of barriers that can be buffered. Default is 32.</param>
internal sealed class ResourceBarrierGroup(
    int size = 32)
{
    private readonly ResourceBarrier[] _barriers = new ResourceBarrier[size];
    private int _offset = 0;

    /// <summary>
    /// Adds Transition barrier to the buffer.
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <param name="flags"></param>
    /// <param name="subresource"></param>
    /// <returns></returns>
    public ResourceBarrierGroup AddTransitionBarrier(
        ID3D12Resource resource,
        ResourceStates before,
        ResourceStates after,
        ResourceBarrierFlags flags = ResourceBarrierFlags.None,
        uint subresource = D3D12.ResourceBarrierAllSubResources)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            _offset,
            _barriers.Length);

        _barriers[_offset++] = ResourceBarrier.BarrierTransition(
            resource: resource,
            stateBefore: before,
            stateAfter: after,
            subresource: subresource,
            flags: flags);

        return this;
    }

    /// <summary>
    /// Adds Unordered Access View (UAV) barrier to the buffer.
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    public ResourceBarrierGroup AddUnorderedAccessViewBarrier(
        ID3D12Resource resource)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            _offset,
            _barriers.Length);

        _barriers[_offset++] = ResourceBarrier.BarrierUnorderedAccessView(
            resource: resource);

        return this;
    }

    /// <summary>
    /// Adds aliasing barrier to the buffer
    /// </summary>
    /// <param name="resourceBefore"></param>
    /// <param name="resourceAfter"></param>
    /// <returns></returns>
    public ResourceBarrierGroup AddAliasingBarrier(
        ID3D12Resource resourceBefore,
        ID3D12Resource resourceAfter)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            _offset,
            _barriers.Length);

        _barriers[_offset++] = ResourceBarrier.BarrierAliasing(
            resourceBefore: resourceBefore,
            resourceAfter: resourceAfter);

        return this;
    }

    /// <summary>
    /// Applies the buffered resource barriers to the command list,
    /// and resets the buffer.
    /// </summary>
    /// <param name="commandList"></param>
    public void Apply(
        ID3D12GraphicsCommandList commandList)
    {
        if (_offset == 0)
        {
            return;
        }

        commandList.ResourceBarrier(_barriers.AsSpan()[.._offset]);
        _offset = 0;
    }
}