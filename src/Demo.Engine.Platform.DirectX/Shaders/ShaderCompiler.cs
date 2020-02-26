using System;
using System.IO;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Models.Enums;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Platform.DirectX.Shaders
{
    public class ShaderCompiler : IShaderCompiler
    {
        private readonly ILogger<ShaderCompiler> _logger;

        public ShaderCompiler(ILogger<ShaderCompiler> logger)
        {
            _logger = logger;
        }

        public ReadOnlyMemory<byte> CompileShader(string path, ShaderStage shaderStage, string entryPoint = "main")
        {
            var shader = File.ReadAllText(path);

            var shaderProfile = $"{GetShaderProfile(shaderStage)}_5_0";
            var filename = Path.GetFileName(path);

            _logger.LogInformation("Compiling {shader} {name} with {profile}", shaderStage, filename, shaderProfile);

            var compileResult = Vortice.D3DCompiler.Compiler.Compile(
                shader,
                entryPoint,
                filename,
                shaderProfile,
                out var blob,
                out var errorBlob
                );

            return compileResult.Failure
                ? throw new Exception(errorBlob?.ConvertToString())
                : blob.GetBytes().AsMemory();
        }

        private static string GetShaderProfile(ShaderStage stage) => stage switch
        {
            ShaderStage.VertexShader => "vs",
            ShaderStage.HullShader => "hs",
            ShaderStage.DomainShader => "ds",
            ShaderStage.GeometryShader => "gs",
            ShaderStage.PixelShader => "ps",
            ShaderStage.ComputeShader => "cs",
            _ => string.Empty,
        };
    }
}