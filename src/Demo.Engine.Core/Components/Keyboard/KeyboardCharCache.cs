// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Text;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Requests.Keyboard;
using Demo.Tools.Common.Collections;
using Demo.Tools.Common.Sys;

namespace Demo.Engine.Core.Components.Keyboard;

/// <summary>
/// Requested through <see cref="KeyboardCharCacheRequest"/>
/// </summary>
public class KeyboardCharCache : IDisposable
{
    private readonly CircularQueue<char> _buffer = new(16);
    private readonly IKeyboardCache _keboardCache;
    private bool _disposedValue = false;

    /// <summary>
    /// <see cref="KeyboardCharCache"/> constructor
    /// </summary>
    /// <param name="keyboardCache">handle to the keyboard cache</param>
    public KeyboardCharCache(IKeyboardCache keyboardCache)
    {
        _keboardCache = keyboardCache;
        _keboardCache.OnChar += KeyboardCache_OnCharEvent;
    }

    private void KeyboardCache_OnCharEvent(object? sender, EventArgs<char> e) => _buffer.Enqueue(e.Value);

    public string ReadCache()
    {
        var sb = new StringBuilder();
        while (_buffer.TryDequeue(out var c))
        {
            _ = sb.Append(c);
        }
        return sb.ToString();
    }

    public void Clear()
    {
        while (_buffer.TryDequeue(out _))
        { }
    }

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _keboardCache.OnChar -= KeyboardCache_OnCharEvent;
                _buffer.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable
}