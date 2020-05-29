using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Models.Enums;
using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D11;

namespace Demo.Engine.Platform.DirectX.Bindable
{
    public class VertexShader : Shader<ID3D11VertexShader>
    {
        public VertexShader(
            string path,
            IShaderCompiler shaderCompiler,
            ID3D11RenderingEngine renderingEngine)
            : base(
                path,
                ShaderStage.VertexShader,
                (device, shader) => device.CreateVertexShader(shader.shaderPointer, shader.shaderLen),
                shaderCompiler,
                renderingEngine)
        {
        }

        public override void Bind() => _renderingEngine.DeviceContext.VSSetShader(_shader);
    }
}