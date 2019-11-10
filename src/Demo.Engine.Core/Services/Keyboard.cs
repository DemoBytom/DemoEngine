using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Demo.Engine.Core.Notifications.Keyboard;
using Demo.Tools.Common.Collections;
using MediatR;

namespace Demo.Engine.Windows.Services
{
    public sealed class Keyboard
        : INotificationHandler<KeyNotification>,
        INotificationHandler<CharNotification>
    {
        private static readonly bool[] _keysPressed = new bool[256];
        private static readonly CircularQueue<char> _chars = new CircularQueue<char>(16);

        public bool KeyPressed(char keyCode) => _keysPressed[keyCode];

        public Keyboard()
        {
        }

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

        //KeyDown/KeyUp
        Task INotificationHandler<KeyNotification>.Handle(KeyNotification notification, CancellationToken cancellationToken)
        {
            _keysPressed[notification.Key] = notification.Down;
            return Task.CompletedTask;
        }

        Task INotificationHandler<CharNotification>.Handle(CharNotification notification, CancellationToken cancellationToken)
        {
            _chars.Enqueue(notification.Char);
            return Task.CompletedTask;
        }
    }
}