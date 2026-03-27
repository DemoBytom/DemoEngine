// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Mediator;

namespace Demo.Engine.Platform.DirectX12.Shaders.Requests;

internal sealed record SaveEngineShadersRequest(
    IAsyncEnumerable<Task<ShaderContent>> Shaders)
    : IRequest<bool>;