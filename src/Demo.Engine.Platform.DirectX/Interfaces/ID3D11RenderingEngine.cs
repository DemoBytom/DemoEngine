// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Rendering;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Interfaces;

public interface ID3D11RenderingEngine : IRenderingEngine
{
    public ID3D11Device Device { get; }
    public ID3D11DeviceContext DeviceContext { get; }
}