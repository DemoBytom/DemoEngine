using System;
using System.Text;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Platform;
using Demo.Tools.Common.Collections;
using Demo.Tools.Common.Sys;

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

        public void Key(VirtualKeys key, bool down) => _keysPressed[(byte)key] = down;

        public void Char(char c)
        {
            _chars.Enqueue(c);
            OnChar?.Invoke(this, new EventArgs<char>(c));
        }

        public ReadOnlyMemory<bool> KeysPressed => _keysPressed.AsMemory();

        public event EventHandler<EventArgs<char>>? OnChar;
    }
}