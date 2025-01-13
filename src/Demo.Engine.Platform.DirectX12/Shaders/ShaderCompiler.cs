// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Platform.DirectX12.Shaders;
internal record ShaderFileInfo(
    string File,
    string Function,
    ShaderId ID,
    ShaderType ShaderType);

/* Shader.bin structure:
 * int shader count
 * int combined shader length
 * [
 *   int ShaderId
 *   int shader length
 *   byte[shaderLength] shader bytecode
 * ]
 * */

internal sealed class ShaderCompiler(
    ILogger<ShaderCompiler> logger,
    IEngineShaderManager engineShaderManager)
    : IShaderAsyncCompiler
{
    private readonly ShaderFileInfo[] _shaderFiles =
    [
        new ShaderFileInfo(
            File: "FullScreenTriangle.hlsl",
            Function: "FullScreenTriangleVS",
            ID: ShaderId.FullscreenTriangle,
            ShaderType: ShaderType.Vertex),
    ];

    private readonly ILogger<ShaderCompiler> _logger = logger;
    private readonly IEngineShaderManager _engineShaderManager = engineShaderManager;

    public ValueTask<bool> CompileShaders(
        CancellationToken cancellationToken = default)
    {
        //TODO: verify compiled shaders are up to date

        foreach (var shaderFile in _shaderFiles)
        {
            _logger.LogInformation("Compiling {shaderFile}",
                shaderFile.File);

            _logger.LogInformation("Attempting {shaderFilePath} shader compliation",
                shaderFile.File);
            //if (_contentFileProvider.FileExists(shaderFile.File))
            //{
            //    // compile shader
            //}
            //else
            //{
            //    return false;
            //}
        }

        return ValueTask.FromResult(true);
    }
}