// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Models.Enums;
using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Bindable.Shaders
{
    public class PixelShader : Shader<ID3D11PixelShader>
    {
        public PixelShader(
            string path,
            IShaderCompiler shaderCompiler,
            ID3D11RenderingEngine renderingEngine)
            : base(
                path,
                ShaderStage.PixelShader,
                (device, shader) => device.CreatePixelShader(shader.shaderPointer, shader.shaderLen),
                shaderCompiler,
                renderingEngine)
        {
        }

        public override void Bind() => _renderingEngine.DeviceContext.PSSetShader(_shader);
    }
}