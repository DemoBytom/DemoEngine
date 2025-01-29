// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using Demo.Engine.Core.Interfaces.Platform;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Platform.DirectX12.Shaders;

internal enum ShaderType
{
    Vertex = 0,
    Hull = 1,
    Domain = 2,
    Geometry = 3,
    Pixel = 4,
    Compute = 5,
    Amplification = 6,
    Mesh = 7,
}

internal enum ShaderId
{
    FullscreenTriangle = 0,
}

internal readonly record struct CompiledShader(
    ShaderId ID,
    int Size,
    ReadOnlyMemory<byte> ShaderBlob);

internal readonly struct ShaderContent
{
    public ShaderContent(
        ShaderId id,
        ReadOnlyMemory<byte> shaderBytecode)
        : this()
    {
        Id = id;
        ShaderBytecode = shaderBytecode;
    }

    public readonly ShaderId Id;
    public readonly ReadOnlyMemory<byte> ShaderBytecode;
}

internal sealed class EngineShaderManager(
    ILogger<EngineShaderManager> logger,
    IContentFileProvider contentFileProvider)
    : IEngineShaderManager,
      IDisposable
{
    private bool _disposedValue;
    private ReadOnlyMemory<byte> _shadersBlob = Array.Empty<byte>();

    private readonly ILogger<EngineShaderManager> _logger = logger;
    private readonly IContentFileProvider _contentFileProvider = contentFileProvider;

    private readonly Dictionary<ShaderId, CompiledShader> _compiledShaders = [];

    public CompiledShader GetShader(
        ShaderId id)
        //TODO Verify shader exists!
        => _compiledShaders[id];

    public const string ENGINE_SHADERS_BIN_FILE = "EngineShaders/shaders.bin";

    public string GetShaderDirAbsolutePath
        => _contentFileProvider.GetAbsolutePath("Shaders");

    public async ValueTask<bool> LoadEngineShaders(
        CancellationToken cancellationToken = default)
    {
        await using var fs = _contentFileProvider.OpenFile(ENGINE_SHADERS_BIN_FILE);
        await ReadShaderData(fs, cancellationToken);

        return true;
    }

    public async Task<bool> SaveEngineShaders(
        IAsyncEnumerable<Task<ShaderContent>> shaders,
        CancellationToken cancellationToken = default)
    {
        await using var shadersBinFile = _contentFileProvider.CreateFile(
            ENGINE_SHADERS_BIN_FILE);
        var pipeWriter = PipeWriter.Create(shadersBinFile);

        await WriteHeader(
            pipeWriter,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var shaderCount = 0;
        var shaderBlobTotalLength = 0;

        await foreach (var shaderTask in shaders)
        {
            var shader = await shaderTask;
            var shaderBytelen = shader.ShaderBytecode.Length;

            WriteShader(
                pipeWriter,
                shader.Id,
                shaderBytelen,
                shader.ShaderBytecode.Span);

            _ = await pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);

            ++shaderCount;
            shaderBlobTotalLength += shaderBytelen;
        }

        _ = shadersBinFile.Seek(0, SeekOrigin.Begin);
        var headerWriter = PipeWriter.Create(shadersBinFile);
        await WriteHeader(
            headerWriter,
            shaderCount,
            shaderBlobTotalLength,
            cancellationToken)
            .ConfigureAwait(false);

        await pipeWriter.CompleteAsync().ConfigureAwait(false);
        await headerWriter.CompleteAsync().ConfigureAwait(false);

        return true;
    }

    private async ValueTask ReadShaderData(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        var pipeReader = PipeReader.Create(stream);
        var shaderCount = await ReadInt32Async(pipeReader, cancellationToken);
        var combinesShaderLength = await ReadInt32Async(pipeReader, cancellationToken);

        Memory<byte> shadersBlob = new byte[combinesShaderLength];
        var currentIndex = 0;
        _shadersBlob = shadersBlob;

        for (var i = 0; i < shaderCount; ++i)
        {
            var shaderId = (ShaderId)await ReadInt32Async(pipeReader, cancellationToken).ConfigureAwait(false);
            var shaderLength = await ReadInt32Async(pipeReader, cancellationToken).ConfigureAwait(false);
            var blobBuffer = shadersBlob.Slice(currentIndex, shaderLength);

            await ReadBlobAsync(
                pipeReader,
                blobBuffer,
                cancellationToken)
                .ConfigureAwait(false);

            _compiledShaders.Add(shaderId,
                new CompiledShader(
                    shaderId,
                    shaderLength,
                    _shadersBlob.Slice(currentIndex, shaderLength)));

            currentIndex += shaderLength;
        }
    }

    private async ValueTask<int> ReadInt32Async(
        PipeReader pipeReader,
        CancellationToken cancellationToken = default)
    {
        var result = await pipeReader.ReadAtLeastAsync(sizeof(int), cancellationToken).ConfigureAwait(false);
        var buffer = result.Buffer;

        if (buffer.Length < sizeof(int))
        {
            _logger.LogError("Not enough data to read 32-bit integer!");
            throw new InvalidOperationException(
                "Not enough data to read 32-bit integer!");
        }

        var span = buffer.FirstSpan;
        var returnValue = BinaryPrimitives.ReadInt32LittleEndian(span);

        pipeReader.AdvanceTo(buffer.GetPosition(sizeof(int)));

        return returnValue;
    }

    private async ValueTask ReadBlobAsync(
        PipeReader reader,
        Memory<byte> destination,
        CancellationToken cancellationToken = default)
    {
        var remainingBytes = destination.Length;
        var writtenBytes = 0;

        while (remainingBytes > 0)
        {
            var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            var buffer = result.Buffer;

            if (buffer.IsEmpty
                && result.IsCompleted)
            {
                _logger.LogError("Unexpected end of stream!");
                throw new InvalidOperationException("Unexpected end of stream!");
            }

            var bytesToCopy = Math.Min(remainingBytes, (int)buffer.Length);
            buffer.Slice(0, bytesToCopy).CopyTo(destination.Span[writtenBytes..]);

            reader.AdvanceTo(buffer.GetPosition(bytesToCopy));
            writtenBytes += bytesToCopy;
            remainingBytes -= bytesToCopy;
        }
    }

    private static async ValueTask WriteHeader(
       PipeWriter pipeWriter,
       int shaderCount = 0,
       int combinedShaderLength = 0,
       CancellationToken cancellationToken = default)
    {
        const int BUFFER_LENGTH = sizeof(int) * 2;
        var headerBuffer = pipeWriter.GetSpan(BUFFER_LENGTH);
        BinaryPrimitives.WriteInt32LittleEndian(headerBuffer, shaderCount);
        BinaryPrimitives.WriteInt32LittleEndian(headerBuffer[sizeof(int)..], combinedShaderLength);

        pipeWriter.Advance(BUFFER_LENGTH);
        _ = await pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
        return;
    }

    private static void WriteShader(
        PipeWriter pipeWriter,
        ShaderId shaderId,
        int shaderLength,
        ReadOnlySpan<byte> shaderBlob)
    {
        WriteInt(in pipeWriter, (int)shaderId);
        WriteInt(in pipeWriter, shaderLength);

        if (shaderBlob.Length > 0)
        {
            var buffer = pipeWriter.GetSpan(shaderLength);
            shaderBlob[..shaderLength].CopyTo(buffer);
            pipeWriter.Advance(shaderLength);
        }

        static void WriteInt(
            in PipeWriter pipeWriter,
            in int value)
        {
            var intSpan = pipeWriter.GetSpan(sizeof(int));
            BinaryPrimitives.WriteInt32LittleEndian(intSpan, value);
            pipeWriter.Advance(sizeof(int));
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _compiledShaders.Clear();
                _shadersBlob = Array.Empty<byte>();
            }

            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~EngineShader()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}