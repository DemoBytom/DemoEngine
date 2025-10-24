// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Platform.DirectX12.RenderingResources;
using Vortice.Direct3D12;

namespace Demo.Engine.Platform.DirectX12;

internal interface ID3D12RenderingEngine : IRenderingEngine
{
    ID3D12Device14 Device { get; }

    ID3D12GraphicsCommandList10 CommandList { get; }
    RTVDescriptorHeapAllocator RTVHeapAllocator { get; }
    SRVDescriptorHeapAllocator SRVHeapAllocator { get; }
    DSVDescriptorHeapAllocator DSVHeapAllocator { get; }
    UAVDescriptorHeapAllocator UAVHeapAllocator { get; }

    void InitCommandList();

    void ExecuteCommandList();

    uint CurrentFrameIndex();

    void SetDeferredReleasesFlag();

    void DeferredRelease(IDisposable disposable);

    T GetRequiredService<T>() where T : notnull;
}