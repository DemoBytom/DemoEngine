using System;
using Demo.Engine.Core.Platform;
using Demo.Tools.Common.Sys;

namespace Demo.Engine.Core.Interfaces.Components
{
    public interface IKeyboardCache
    {
        event EventHandler<EventArgs<char>> OnChar;

        ReadOnlyMemory<bool> KeysPressed { get; }

        void Char(char c);

        void ClearState();

        void Key(VirtualKeys key, bool down);

        string ReadChars();
    }
}