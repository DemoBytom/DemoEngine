// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Platform;

namespace Demo.Engine.Core.Components.Keyboard;

public class KeyboardHandle
{
    private readonly IKeyboardCache _keyboardCache;

    public KeyboardHandle(
        IKeyboardCache keyboardCache) => _keyboardCache = keyboardCache;

    /// <summary>
    /// Returns whether a provided key is pressed down or not
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <returns>true if key is down</returns>
    public bool GetKeyPressed(VirtualKeys key) => _keyboardCache.KeysPressed.Span[(byte)key];
}