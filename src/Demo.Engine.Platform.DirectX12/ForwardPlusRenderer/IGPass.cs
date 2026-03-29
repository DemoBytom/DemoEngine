// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.ValueObjects;
using Demo.Engine.Platform.DirectX12.RenderingResources;
using Vortice.Direct3D12;

namespace Demo.Engine.Platform.DirectX12.ForwardPlusRenderer;

internal interface IGPass
{
    void AddTransitionForDepthPrepass(ResourceBarrierGroup barriers);
    void AddTransitionForGPass(ResourceBarrierGroup barriers);
    void AddTranstionForPostProcess(ResourceBarrierGroup barriers);
    void DepthPrepass(ID3D12GraphicsCommandList commandList, in FrameInfo frameInfo);
    void Render(ID3D12GraphicsCommandList commandList, in FrameInfo frameInfo);
    void SetRenderTargetsForDepthPrepass(ID3D12GraphicsCommandList commandList);
    void SetRenderTargetsForGPass(ID3D12GraphicsCommandList commandList);
    void SetSize(Width width, Height height);
}