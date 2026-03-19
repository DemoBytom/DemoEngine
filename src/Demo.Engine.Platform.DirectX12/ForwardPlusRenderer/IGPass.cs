// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.ValueObjects;
using Vortice.Direct3D12;

namespace Demo.Engine.Platform.DirectX12.ForwardPlusRenderer;

internal interface IGPass
{
    void DepthPrepass(ID3D12GraphicsCommandList commandList, in FrameInfo frameInfo);
    void Render(ID3D12GraphicsCommandList commandList, in FrameInfo frameInfo);
    void SetSize(Width width, Height height);
}