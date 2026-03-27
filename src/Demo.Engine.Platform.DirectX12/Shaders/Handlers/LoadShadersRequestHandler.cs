// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Requests;
using Mediator;

namespace Demo.Engine.Platform.DirectX12.Shaders.Handlers;

public sealed class LoadShadersRequestHandler(
    IEngineShaderManager engineShaderManager)
        : IRequestHandler<LoadShadersRequest, bool>
{
    private readonly IEngineShaderManager _engineShaderManager = engineShaderManager;

    public async ValueTask<bool> Handle(
        LoadShadersRequest request,
        CancellationToken cancellationToken)
        => await _engineShaderManager.LoadEngineShaders(cancellationToken);
}