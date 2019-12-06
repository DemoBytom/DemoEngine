using System;
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

        public KeyboardHandleResponse(ReadOnlyMemory<bool> keyboardState)
        {
            _keyboardState = keyboardState;
        }

        public bool GetKeyPressed(VirtualKeys key) => _keyboardState.Span[(byte)key];
    }
}