// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.DirectX12.Shaders;

internal enum ShaderType
{
    Vertex = 0,
    Hull,
    Domain,
    Geometry,
    Pixel,
    Compute,
    Amplification,
    Mesh,
}

internal enum ShaderId
{
    FullscreenTriangle,
}

internal record CompiledShader(
    int Size,
    ReadOnlyMemory<byte> ShaderBlob);

internal record EngineShader
    : IDisposable
{
    private bool _disposedValue;
    private ReadOnlyMemory<byte> _shadersBlob;

    public CompiledShader[] CompiledShaders { get; }

    public EngineShader()
        => CompiledShaders = [];

    private bool TryLoadEngineShaders(
        out ReadOnlyMemory<byte> shadersBlob,
        out ushort size)
        => throw new NotImplementedException();

    public bool LoadEngineShaders()
    {
        var result = TryLoadEngineShaders(
            out _shadersBlob,
            out var size);

        ulong offset = 0;

        while (offset < size && result)
        {
        }

        return result;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
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