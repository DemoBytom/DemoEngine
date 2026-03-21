// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.DirectX12.Shaders;

internal interface IShaderAsyncCompiler
{
    Task<bool> CompileShaders(CancellationToken cancellationToken = default);
}