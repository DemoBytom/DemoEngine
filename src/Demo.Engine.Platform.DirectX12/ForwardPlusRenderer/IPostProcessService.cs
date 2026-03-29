// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Vortice.Direct3D12;

namespace Demo.Engine.Platform.DirectX12.ForwardPlusRenderer;

internal interface IPostProcessService
    : IDisposable
{
    bool Initialize();
    void PostProcess(ID3D12GraphicsCommandList commandList, CpuDescriptorHandle targetRTV);
}