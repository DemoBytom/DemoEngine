using System;
using Demo.Engine.Core.Models.Enums;

namespace Demo.Engine.Core.Interfaces.Rendering.Shaders
{
    public interface IShaderCompiler
    {
        ReadOnlyMemory<byte> CompileShader(string path, ShaderStage shaderStage, string entryPoint = "main");
    }
}