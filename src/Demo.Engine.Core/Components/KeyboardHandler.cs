using System.Threading;
using System.Threading.Tasks;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Notifications.Keyboard;
using Demo.Engine.Core.Requests.Keyboard;
using MediatR;

namespace Demo.Engine.Core.Components
{
    public sealed class KeyboardHandler :
        INotificationHandler<KeyNotification>,
        INotificationHandler<CharNotification>,
        INotificationHandler<ClearKeysNotification>,
        IRequestHandler<KeyboardHandleRequest, KeyboardHandleResponse>,
        IRequestHandler<KeyboardCharRequest, KeyboardCharResponse>
    {
        private readonly IKeyboardCache _keyboardCache;

        public KeyboardHandler(IKeyboardCache keyboardCache)
        {
            _keyboardCache = keyboardCache;
        }

        /// <summary>
        /// OnKeyUp/OnKeyDown
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task INotificationHandler<KeyNotification>.Handle(KeyNotification notification, CancellationToken cancellationToken)
        {
            _keyboardCache.Key(notification.Key, notification.Down);
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
            _keyboardCache.Char(notification.Char);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Clean keys cache
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task INotificationHandler<ClearKeysNotification>.Handle(ClearKeysNotification notification, CancellationToken cancellationToken)
        {
            _keyboardCache.ClearState();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get the keyboard handle
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<KeyboardHandleResponse> IRequestHandler<KeyboardHandleRequest, KeyboardHandleResponse>.Handle(KeyboardHandleRequest request, CancellationToken cancellationToken)
        {
            var response = new KeyboardHandleResponse(
                _keyboardCache);
            return Task.FromResult(response);
        }

        Task<KeyboardCharResponse> IRequestHandler<KeyboardCharRequest, KeyboardCharResponse>.Handle(KeyboardCharRequest request, CancellationToken cancellationToken)
        {
            var response = new KeyboardCharResponse(_keyboardCache);
            return Task.FromResult(response);
        }
    }
}