// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Platform.DirectX12.Shaders.Requests;
using Mediator;

namespace Demo.Engine.Platform.DirectX12.Shaders.Handlers;

internal sealed class SaveEngineShadersRequestHandler(
    IEngineShaderManager engineShaderManager)
    : IRequestHandler<SaveEngineShadersRequest, bool>
{
    private readonly IEngineShaderManager _engineShaderManager = engineShaderManager;

    public async ValueTask<bool> Handle(
        SaveEngineShadersRequest request,
        CancellationToken cancellationToken)
        => await _engineShaderManager.SaveEngineShaders(request.Shaders, cancellationToken);
}