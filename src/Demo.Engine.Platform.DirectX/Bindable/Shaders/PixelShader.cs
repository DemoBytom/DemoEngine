// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Platform.DirectX.Interfaces;
using Demo.Engine.Platform.DirectX.Shaders;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Bindable.Shaders;

public class PixelShader : Shader<ID3D11PixelShader>
{
    public PixelShader(
        CompiledS compiledS,
        ID3D11RenderingEngine renderingEngine)
        : base(
            compiledS,
            (device, shader) => device.CreatePixelShader(shader.shaderPointer, shader.shaderLen),
            renderingEngine)
    {
    }

    public override void Bind() => _renderingEngine.DeviceContext.PSSetShader(_shader);
}