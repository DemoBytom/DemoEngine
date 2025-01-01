// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Rendering;
using Vortice.Direct3D12;

namespace Demo.Engine.Platform.DirectX12;

public interface ID3D12RenderingEngine : IRenderingEngine
{
    public ID3D12Device10 Device { get; }

    public ID3D12GraphicsCommandList7 CommandList { get; }

    public void InitCommandList();

    public void ExecuteCommandList();
    void SignalAndWait();
}