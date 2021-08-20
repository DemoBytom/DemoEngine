// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System;
using Demo.Engine.Core.Platform;
using Demo.Tools.Common.Sys;

namespace Demo.Engine.Core.Interfaces.Components
{
    /// <summary>
    /// A cache holding information about current keyboard state
    /// </summary>
    public interface IKeyboardCache
    {
        /// <summary>
        /// OnChar event that can be subscribed to, that's fired whenever OnChar event is emited
        /// from the keyboard
        /// </summary>
        event EventHandler<EventArgs<char>> OnChar;

        /// <summary>
        /// Essentially an array holding info about all keys and whether or not they are pressed down
        /// </summary>
        ReadOnlyMemory<bool> KeysPressed { get; }

        /// <summary>
        /// Fires <see cref="OnChar"/> for all it's subscribers
        /// </summary>
        /// <param name="c">A <see cref="char"/> that was registered by the keyboard</param>
        void Char(char c);

        /// <summary>
        /// Clears the <see cref="KeysPressed"/> signaling that no key is pressed
        /// </summary>
        void ClearState();

        /// <summary>
        /// Sets the state of the provided <paramref name="key"/>
        /// </summary>
        /// <param name="key">Key which's state is being set</param>
        /// <param name="down">Whether the provided <paramref name="key"/> is up or down</param>
        void Key(VirtualKeys key, bool down);
    }
}