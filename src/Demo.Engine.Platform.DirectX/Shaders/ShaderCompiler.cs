// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Models.Enums;
using Microsoft.Extensions.Logging;
using Vortice.Direct3D;
using Compiler = Vortice.D3DCompiler.Compiler;

namespace Demo.Engine.Platform.DirectX.Shaders;

public class ShaderCompiler : IShaderCompiler
{
    private readonly ILogger<ShaderCompiler> _logger;

    public ShaderCompiler(ILogger<ShaderCompiler> logger) =>
        _logger = logger;

    public ReadOnlyMemory<byte> CompileShader(string path, ShaderStage shaderStage, string entryPoint = "main")
    {
        var shader = File.ReadAllText(path);

        var shaderProfile = $"{GetShaderProfile(shaderStage)}_5_0";
        var filename = Path.GetFileName(path);

        _logger.LogInformation("Compiling {shader} {name} with {profile}", shaderStage, filename, shaderProfile);
        Blob? blob = null;
        Blob? errorBlob = null;
        try
        {
            var compileResult = Compiler.Compile(
                shader,
                entryPoint,
                filename,
                shaderProfile,
                out blob,
                out errorBlob
                );

            return compileResult.Failure
                ? throw new Exception(errorBlob?.AsString())
                : blob.AsBytes().AsMemory();
        }
        finally
        {
            blob?.Dispose();
            errorBlob?.Dispose();
        }
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

public record CompiledS
{
    public CompiledS(ReadOnlyMemory<byte> compiledShader)
        => CompiledShader = compiledShader;
    public ReadOnlyMemory<byte> CompiledShader { get; }
}
public record CompiledVS : CompiledS
{
    public CompiledVS(
        string path,
        IShaderCompiler shaderCompiler)
        : base(shaderCompiler.CompileShader(
            path,
            ShaderStage.VertexShader))
    {
    }
}

public record CompiledPS : CompiledS
{
    public CompiledPS(
        string path,
        IShaderCompiler shaderCompiler)
        : base(shaderCompiler.CompileShader(
            path,
            ShaderStage.PixelShader))
    {
    }
}