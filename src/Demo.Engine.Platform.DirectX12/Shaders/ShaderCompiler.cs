// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Microsoft.Extensions.Logging;
using SharpGen.Runtime;
using Vortice.Dxc;

namespace Demo.Engine.Platform.DirectX12.Shaders;

internal record struct ShaderFileInfo(
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

    public async Task<bool> CompileShaders(
        CancellationToken cancellationToken = default)
    {
        //TODO: verify compiled shaders are up to date
        var compilationTasks = ArrayPool<Task<ShaderContent>>
            .Shared
            .Rent(_shaderFiles.Length);

        var completed = false;
        try
        {
            for (var i = 0; i < _shaderFiles.Length; ++i)
            {
                var shaderFile = _shaderFiles[i];
                _logger.LogInformation("Compiling {shaderFile}",
                    shaderFile.File);

                _logger.LogInformation("Attempting {shaderFilePath} shader compliation",
                    shaderFile.File);
                var compilationTask = CompileAShader(shaderFile, cancellationToken);
                compilationTasks[i] = compilationTask;
            }

            _ = await _engineShaderManager.SaveEngineShaders(
                    Task.WhenEach<ShaderContent>(
                        compilationTasks
                            .AsSpan()[.._shaderFiles.Length]),
                cancellationToken)
                .ConfigureAwait(false);

            completed = true;
        }
        finally
        {
            if (!completed)
            {
                _ = await Task.WhenAll<ShaderContent>(
                    compilationTasks
                        .AsSpan()[.._shaderFiles.Length])
                    .ConfigureAwait(false);
            }
            ArrayPool<Task<ShaderContent>>
                .Shared
                .Return(compilationTasks);
        }

        return true;
    }

    private Task<ShaderContent> CompileAShader(
        ShaderFileInfo shaderFileInfo,
        CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(
            _engineShaderManager.GetShaderDirAbsolutePath,
            shaderFileInfo.File);

        using var fs = new FileStream(
            path, FileMode.Open, FileAccess.Read);
        using var sr = new StreamReader(fs);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(30));

        var shaderSource = sr.ReadToEnd();

        using var result = DxcCompiler.Compile(
            shaderFileInfo.ShaderType switch
            {
                ShaderType.Vertex => DxcShaderStage.Vertex,
                ShaderType.Hull => DxcShaderStage.Hull,
                ShaderType.Domain => DxcShaderStage.Domain,
                ShaderType.Geometry => DxcShaderStage.Geometry,
                ShaderType.Pixel => DxcShaderStage.Pixel,
                ShaderType.Compute => DxcShaderStage.Compute,
                ShaderType.Amplification => DxcShaderStage.Amplification,
                ShaderType.Mesh => DxcShaderStage.Mesh,
                _ => throw new UnreachableException()
            },
            shaderSource,
            shaderFileInfo.Function,
            new DxcCompilerOptions
            {
                ShaderModel = DxcShaderModel.Model6_4,
            });

        if (result.GetStatus().Failure)
        {
            var exception = new Exception(result.GetErrors());
            _logger.LogCritical(
                exception,
                "Fatal error when compiling {shader}!",
                shaderFileInfo.File);
            throw exception;
        }

        var memory = result.GetObjectBytecodeMemory();

        var shaderContent = new ShaderContent(
            shaderFileInfo.ID,
            memory);

        return Task.FromResult(shaderContent);
    }

    //return Task.FromResult<ShaderContent>(default);

    private static async IAsyncEnumerable<T> AwaitEach<T>(
        IAsyncEnumerable<Task<T>> input,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var task in input
            .WithCancellation(cancellationToken))
        {
            yield return await task;
        }
    }

    private sealed class ShaderIncludeHandler
        : CallbackBase,
          IDxcIncludeHandler
    {
        public Result LoadSource(
            string filename,
            out IDxcBlob includeSource)
            => throw new NotImplementedException();
    }
}