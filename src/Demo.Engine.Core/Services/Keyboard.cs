using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Demo.Engine.Core.Notifications.Keyboard;
using Demo.Engine.Core.Requests.Keyboard;
using Demo.Tools.Common.Collections;
using MediatR;

namespace Demo.Engine.Windows.Services
{
    public sealed class Keyboard :
        INotificationHandler<KeyNotification>,
        INotificationHandler<CharNotification>,
        INotificationHandler<ClearKeysNotification>,
        IRequestHandler<KeyboardStateRequest, KeyboardStateResponse>
    {
        private static readonly bool[] _keysPressed = new bool[256];
        private static readonly CircularQueue<char> _chars = new CircularQueue<char>(16);

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

        /// <summary>
        /// OnKeyUp/OnKeyDown
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task INotificationHandler<KeyNotification>.Handle(KeyNotification notification, CancellationToken cancellationToken)
        {
            _keysPressed[(byte)notification.Key] = notification.Down;
            return Task.CompletedTask;
        }

        /// <summary>
        /// OnChar
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task INotificationHandler<CharNotification>.Handle(CharNotification notification, CancellationToken cancellationToken)
        {
            _chars.Enqueue(notification.Char);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Query keyboard state
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<KeyboardStateResponse> IRequestHandler<KeyboardStateRequest, KeyboardStateResponse>.Handle(KeyboardStateRequest request, CancellationToken cancellationToken)
        {
            var response = new KeyboardStateResponse(_keysPressed);
            return Task.FromResult(response);
        }

        Task INotificationHandler<ClearKeysNotification>.Handle(ClearKeysNotification notification, CancellationToken cancellationToken)
        {
            ClearState();
            return Task.CompletedTask;
        }
    }
}