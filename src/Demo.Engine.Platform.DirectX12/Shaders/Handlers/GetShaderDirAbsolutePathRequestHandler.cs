// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Platform.DirectX12.Shaders.Requests;
using Mediator;

namespace Demo.Engine.Platform.DirectX12.Shaders.Handlers;

internal sealed class GetShaderDirAbsolutePathRequestHandler(
    IEngineShaderManager engineShaderManager)
    : IRequestHandler<GetShaderDirAbsolutePathRequest, string>
{
    private readonly IEngineShaderManager _engineShaderManager = engineShaderManager;

    public ValueTask<string> Handle(
        GetShaderDirAbsolutePathRequest request,
        CancellationToken cancellationToken)
        => ValueTask.FromResult(_engineShaderManager.GetShaderDirAbsolutePath);
}