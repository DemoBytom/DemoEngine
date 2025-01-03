// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Platform.DirectX12.RenderingResources;
using Vortice.Direct3D12;

namespace Demo.Engine.Platform.DirectX12;

internal interface ID3D12RenderingEngine : IRenderingEngine
{
    public ID3D12Device14 Device { get; }

    public ID3D12GraphicsCommandList10 CommandList { get; }
    public RTVDescriptorHeapAllocator RTVHeapAllocator { get; }
    public SRVDescriptorHeapAllocator SRVHeapAllocator { get; }
    public DSVDescriptorHeapAllocator DSVHeapAllocator { get; }
    public UAVDescriptorHeapAllocator UAVHeapAllocator { get; }

    public void InitCommandList();

    public void ExecuteCommandList();

    public uint CurrentFrameIndex();

    public void SetDeferredReleasesFlag();

    void DeferredRelease(IDisposable disposable);

    T GetRequiredService<T>() where T : notnull;
}