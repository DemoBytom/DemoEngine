// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.DirectX12.Shaders;

public interface IEngineShaderManager
{
    internal string GetShaderDirAbsolutePath { get; }

    internal CompiledShader GetShader(ShaderId id);

    internal ValueTask<bool> LoadEngineShaders(CancellationToken cancellationToken = default);

    internal Task<bool> SaveEngineShaders(IAsyncEnumerable<Task<ShaderContent>> shaders, CancellationToken cancellationToken = default);
}