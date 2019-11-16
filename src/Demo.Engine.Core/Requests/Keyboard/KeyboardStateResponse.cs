using System;
using System.Linq;
using Demo.Engine.Core.Platform;

namespace Demo.Engine.Core.Requests.Keyboard
{
    /// <summary>
    /// Current snapshot of a keyboard state
    /// </summary>
    public class KeyboardStateResponse
    {
        private readonly bool[] _keysPressed;

        public KeyboardStateResponse(
            bool[] keysPressed)
        {
            _keysPressed = keysPressed;
            //Cache currently pressed keys in case we need it
            _getPressed = new Lazy<ReadOnlyMemory<VirtualKeys>>(() => _keysPressed
               .Select((keyDown, i) => (keyDown, i))
               .Where(o => o.keyDown)
               .Select(o => (VirtualKeys)o.i)
               .ToArray()
               .AsMemory());
        }

        /// <summary>
        /// Checks the state of a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true if provided <see cref="VirtualKeys"/> is pressed down, false otherwise</returns>
        public bool GetKeyState(VirtualKeys key) => _keysPressed[(byte)key];

        /// <summary>
        /// Returns an array with all currently pressed down keys
        /// </summary>
        /// <returns><see cref="VirtualKeys"/> array containing currently pressed down keys</returns>
        public ReadOnlyMemory<VirtualKeys> GetPressedKeys() => _getPressed.Value;

        private readonly Lazy<ReadOnlyMemory<VirtualKeys>> _getPressed;
    }
}