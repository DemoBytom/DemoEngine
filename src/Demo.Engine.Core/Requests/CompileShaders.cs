// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Mediator;

namespace Demo.Engine.Core.Requests;

public sealed record CompileShaders
    : IRequest<bool>
{
}