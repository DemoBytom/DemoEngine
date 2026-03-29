// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Demo.Engine.Core.Requests;
using Demo.Engine.Platform.DirectX12.Shaders.Requests;
using Mediator;
using Microsoft.Extensions.Logging;
using SharpGen.Runtime;
using Vortice.Dxc;

namespace Demo.Engine.Platform.DirectX12.Shaders;

internal record struct ShaderFileInfo(
    string File,
    string Function,
    ShaderId ID,
    ShaderType ShaderType);

internal sealed class ShaderCompiler(
    ILogger<ShaderCompiler> logger,
    IMediator mediator)
    : IShaderAsyncCompiler,
      IRequestHandler<CompileShaders, bool>
{
    private readonly ShaderFileInfo[] _shaderFiles =
    [
        new ShaderFileInfo(
            File: Path.Combine("ShaderFiles", "FullScreenTriangle.hlsl"),
            Function: "FullScreenTriangleVS",
            ID: ShaderId.FullscreenTriangleVS,
            ShaderType: ShaderType.Vertex),
        new ShaderFileInfo(
            File: Path.Combine("ShaderFiles", "FillColor.hlsl"),
            Function: "FillColorPS",
            ID: ShaderId.FillColorPS,
            ShaderType: ShaderType.Pixel),
        new ShaderFileInfo(
            File: Path.Combine("Triangle", "TrianglePS.hlsl"),
            Function: "main",
            ID: ShaderId.TrianglePS,
            ShaderType: ShaderType.Pixel),
        new ShaderFileInfo(
            File: Path.Combine("Triangle", "TriangleVS.hlsl"),
            Function: "main",
            ID: ShaderId.TriangleVS,
            ShaderType: ShaderType.Vertex),
    ];

    private readonly ILogger<ShaderCompiler> _logger = logger;
    private readonly IMediator _mediator = mediator;

    private static readonly GetShaderDirAbsolutePathRequest _getShaderDirAbsolutePathRequest = new();

    public async ValueTask<bool> Handle(
        CompileShaders request,
        CancellationToken cancellationToken)
        => await CompileShaders(cancellationToken);

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
                _logger.LogCompilingShaderFile(
                    shaderFile.File,
                    shaderFile.ID,
                    shaderFile.ShaderType);

                var compilationTask = CompileAShader(shaderFile, cancellationToken);
                compilationTasks[i] = compilationTask;
            }

            _ = await _mediator.Send(new SaveEngineShadersRequest(
                Task.WhenEach(
                    compilationTasks
                        .AsSpan()[.._shaderFiles.Length])),
                    cancellationToken);

            completed = true;
        }
        finally
        {
            if (!completed)
            {
                _ = await Task.WhenAll(
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

    private async Task<ShaderContent> CompileAShader(
        ShaderFileInfo shaderFileInfo,
        CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(
            await _mediator.Send(_getShaderDirAbsolutePathRequest, cancellationToken),
            shaderFileInfo.File);

        using var fs = new FileStream(
            path, FileMode.Open, FileAccess.Read);
        using var sr = new StreamReader(fs);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(30));

        var shaderSource = await sr.ReadToEndAsync(linkedCts.Token);

        using var result = DxcCompiler.Compile(
            shaderFileInfo.ShaderType.ToShaderStage(),
            shaderSource,
            shaderFileInfo.Function,
            new DxcCompilerOptions
            {
                ShaderModel = DxcShaderModel.Model6_4,
            });

        if (result.GetStatus().Failure)
        {
            var exception = new Exception(result.GetErrors());
            _logger.LogErrorCompilingShader(
                exception,
                shaderFileInfo.File);
            throw exception;
        }

        var memory = result.GetObjectBytecodeMemory();

        var shaderContent = new ShaderContent(
            shaderFileInfo.ID,
            memory);

        return shaderContent;
    }

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

file static class SomeExtensions
{
    extension(ShaderType shaderType)
    {
        public DxcShaderStage ToShaderStage() => shaderType switch
        {
            ShaderType.Vertex => DxcShaderStage.Vertex,
            ShaderType.Hull => DxcShaderStage.Hull,
            ShaderType.Domain => DxcShaderStage.Domain,
            ShaderType.Geometry => DxcShaderStage.Geometry,
            ShaderType.Pixel => DxcShaderStage.Pixel,
            ShaderType.Compute => DxcShaderStage.Compute,
            ShaderType.Amplification => DxcShaderStage.Amplification,
            ShaderType.Mesh => DxcShaderStage.Mesh,
            _ => throw new UnreachableException(),
        };
    }
}