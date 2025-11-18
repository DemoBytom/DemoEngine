// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Models.Enums;

namespace Demo.Engine.Core.Interfaces.Rendering.Shaders;

public interface IShaderCompiler
{
    ReadOnlyMemory<byte> CompileShader(string path, ShaderStage shaderStage, string entryPoint = "main");
}

public interface IShaderAsyncCompiler
{
    Task<bool> CompileShaders(CancellationToken cancellationToken = default);
}