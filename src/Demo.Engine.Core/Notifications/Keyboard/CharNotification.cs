// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Mediator;

namespace Demo.Engine.Core.Notifications.Keyboard;

public class CharNotification : INotification
{
    public CharNotification(char @char) => Char = @char;

    public char Char { get; }
}