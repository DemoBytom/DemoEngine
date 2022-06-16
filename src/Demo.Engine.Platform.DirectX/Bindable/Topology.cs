// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D;

namespace Demo.Engine.Platform.DirectX.Bindable;

public class Topology : IBindable
{
    private readonly ID3D11RenderingEngine _renderingEngine;
    private readonly PrimitiveTopology _topology;

    public Topology(
        ID3D11RenderingEngine renderingEngine,
        PrimitiveTopology topology)
    {
        _renderingEngine = renderingEngine;
        _topology = topology;
    }

    public void Bind() => _renderingEngine.DeviceContext.IASetPrimitiveTopology(_topology);
}