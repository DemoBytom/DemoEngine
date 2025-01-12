// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Platform.DirectX12.Shaders;
internal record ShaderFileInfo(
    string File,
    string Function,
    ShaderId ID,
    ShaderType ShaderType);

internal class ShaderCompiler
{
    private readonly ShaderFileInfo[] _shaderFiles =
    [
        new ShaderFileInfo(
            File: "FullScreenTriangle.hlsl",
            Function: "FullScreenTriangleVS",
            ID: ShaderId.FullscreenTriangle,
            ShaderType: ShaderType.Vertex),
    ];
    private readonly string _rootPath;
    private readonly string _shaderDirPath;
    private readonly ILogger<ShaderCompiler> _logger;

    public ShaderCompiler(
        ILogger<ShaderCompiler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;

        _rootPath = environment.ContentRootPath;
        _shaderDirPath = Path.Combine(
            _rootPath,
            "Shaders");

        _logger.LogInformation("Shader file path {shaderDirPath}",
            _shaderDirPath);
    }
    public bool CompileShaders()
    {
        //verify compiled shaders are up to date

        List<ReadOnlyMemory<byte>> shaders = [];

        foreach (var shaderFile in _shaderFiles)
        {
            var path = Path.Combine(_shaderDirPath, shaderFile.File);
            _logger.LogInformation("Attempting {shaderFilePath} shader compliation",
                path);
            if (File.Exists(path))
            {
                // compile shader
                shaders.Add(Array.Empty<byte>());
            }
            else
            {
                return false;
            }
        }

        //save compiled files
        return SaveCompiledShaders();
    }

    private bool SaveCompiledShaders()
    {
    }
}