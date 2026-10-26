// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.
using Demo.Engine.Core.Platform;

namespace Demo.Engine.Platform.Windows.MessageOperations;

internal readonly ref struct KeyOperation(
    in Message msg)
{
    public readonly ref readonly Message Message = ref msg;

    public VirtualKeys Key => (VirtualKeys)Message.WParam;
}