// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Platform;
using Demo.Tools.Common.Sys;

namespace Demo.Engine.Core.Components.Keyboard.Internal;

public class KeyboardCache : IKeyboardCache
{
    private static readonly bool[] _keysPressed = new bool[256];

    public void ClearState() =>
        Array.Clear(
            _keysPressed,
            0,
            _keysPressed.Length);

    public void Key(VirtualKeys key, bool down) => _keysPressed[(byte)key] = down;

    public void Char(char c) => OnChar?.Invoke(this, new EventArgs<char>(c));

    public ReadOnlyMemory<bool> KeysPressed => _keysPressed.AsMemory();

    public event EventHandler<EventArgs<char>>? OnChar;
}