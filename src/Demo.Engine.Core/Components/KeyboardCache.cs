using System;
using System.Text;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Platform;
using Demo.Tools.Common.Collections;

namespace Demo.Engine.Core.Components
{
    public class KeyboardCache : IKeyboardCache
    {
        private static readonly bool[] _keysPressed = new bool[256];
        private static readonly CircularQueue<char> _chars = new CircularQueue<char>(16);

        public void ClearState() =>
            Array.Clear(
                _keysPressed,
                0,
                _keysPressed.Length);

        public string ReadChars()
        {
            var sb = new StringBuilder();
            while (_chars.TryDequeue(out var c))
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        public void OnKey(VirtualKeys key, bool down) => _keysPressed[(byte)key] = down;

        public void OnChar(char c) => _chars.Enqueue(c);

        public ReadOnlyMemory<bool> KeysPressed => _keysPressed.AsMemory();
    }
}