using System;
using Demo.Engine.Core.Platform;

namespace Demo.Engine.Core.Interfaces.Components
{
    public interface IKeyboardCache
    {
        ReadOnlyMemory<bool> KeysPressed { get; }

        void OnChar(char c);

        void ClearState();

        void OnKey(VirtualKeys key, bool down);

        string ReadChars();
    }
}