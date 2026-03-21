// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Notifications.Keyboard;
using Demo.Engine.Core.Requests.Keyboard;
using Mediator;

namespace Demo.Engine.Core.Components.Keyboard.Internal;

public sealed class KeyboardHandler :
    INotificationHandler<KeyNotification>,
    INotificationHandler<CharNotification>,
    INotificationHandler<ClearKeysNotification>,
    IRequestHandler<KeyboardHandleRequest, KeyboardHandle>,
    IRequestHandler<KeyboardCharCacheRequest, KeyboardCharCache>
{
    private readonly IKeyboardCache _keyboardCache;

    public KeyboardHandler(IKeyboardCache keyboardCache) => _keyboardCache = keyboardCache;

    /// <summary>
    /// OnKeyUp/OnKeyDown
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public ValueTask Handle(KeyNotification notification, CancellationToken cancellationToken)
    {
        _keyboardCache.Key(notification.Key, notification.Down);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// OnChar
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public ValueTask Handle(CharNotification notification, CancellationToken cancellationToken)
    {
        _keyboardCache.Char(notification.Char);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Clean keys cache
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public ValueTask Handle(ClearKeysNotification notification, CancellationToken cancellationToken)
    {
        _keyboardCache.ClearState();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Get the keyboard handle
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public ValueTask<KeyboardHandle> Handle(KeyboardHandleRequest request, CancellationToken cancellationToken)
    {
        var response = new KeyboardHandle(
            _keyboardCache);
        return ValueTask.FromResult(response);
    }

    public ValueTask<KeyboardCharCache> Handle(KeyboardCharCacheRequest request, CancellationToken cancellationToken)
    {
        var response = new KeyboardCharCache(_keyboardCache);
        return ValueTask.FromResult(response);
    }
}