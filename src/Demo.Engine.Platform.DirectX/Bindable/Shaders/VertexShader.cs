// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Platform.DirectX.Interfaces;
using Demo.Engine.Platform.DirectX.Shaders;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Bindable.Shaders;

public class VertexShader : Shader<ID3D11VertexShader>
{
    public VertexShader(
        CompiledVS compiledVS,
        ID3D11RenderingEngine renderingEngine)
        : base(
            compiledVS,
            (device, shader) => device.CreateVertexShader(shader.CompiledShader.Span),
            renderingEngine)
    {
    }

    public override void Bind() => _renderingEngine.DeviceContext.VSSetShader(_shader);
}