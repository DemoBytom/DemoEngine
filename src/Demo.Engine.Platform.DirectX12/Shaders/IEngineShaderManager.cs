// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.DirectX12.Shaders;

internal interface IEngineShaderManager
{
    CompiledShader GetShader(ShaderId id);

    ValueTask<bool> LoadEngineShaders(CancellationToken cancellationToken = default);
}