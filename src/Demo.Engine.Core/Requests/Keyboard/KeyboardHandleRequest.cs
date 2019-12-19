using System;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Platform;
using MediatR;

namespace Demo.Engine.Core.Requests.Keyboard
{
    public class KeyboardHandleRequest : IRequest<KeyboardHandleResponse>
    {
    }

    public class KeyboardHandleResponse
    {
        private readonly ReadOnlyMemory<bool> _keyboardState;
        private readonly IKeyboardCache _keyboardCache;

        public KeyboardHandleResponse(
            IKeyboardCache keyboardCache)
        {
            _keyboardState = keyboardCache.KeysPressed;
            _keyboardCache = keyboardCache;
        }

        public bool GetKeyPressed(VirtualKeys key) => _keyboardState.Span[(byte)key];

        public string GetString() => _keyboardCache.ReadChars();
    }
}