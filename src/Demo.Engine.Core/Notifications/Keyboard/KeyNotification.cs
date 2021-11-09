// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Platform;
using MediatR;

namespace Demo.Engine.Core.Notifications.Keyboard;

public class KeyNotification : INotification
{
    public KeyNotification(VirtualKeys key, bool down)
    {
        Key = key;
        Down = down;
    }

    public VirtualKeys Key { get; }
    public bool Down { get; }
}