// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Components.Keyboard;
using MediatR;

namespace Demo.Engine.Core.Requests.Keyboard;

public class KeyboardHandleRequest : IRequest<KeyboardHandle>
{
}