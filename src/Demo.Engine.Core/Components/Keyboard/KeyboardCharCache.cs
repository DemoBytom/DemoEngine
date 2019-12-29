using System;
using System.Text;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Requests.Keyboard;
using Demo.Tools.Common.Collections;
using Demo.Tools.Common.Sys;

namespace Demo.Engine.Core.Components.Keyboard
{
    /// <summary>
    /// Requested through <see cref="KeyboardCharCacheRequest"/>
    /// </summary>
    public class KeyboardCharCache : IDisposable
    {
        private readonly CircularQueue<char> _buffer = new CircularQueue<char>(16);
        private readonly IKeyboardCache _keboardCache;

        /// <summary>
        /// <see cref="KeyboardCharCache"/> constructor
        /// </summary>
        /// <param name="keyboardCache">handle to the keyboard cache</param>
        public KeyboardCharCache(IKeyboardCache keyboardCache)
        {
            _keboardCache = keyboardCache;
            _keboardCache.OnChar += KeyboardCache_OnCharEvent;
        }

        private void KeyboardCache_OnCharEvent(object sender, EventArgs<char> e)
        {
            _buffer.Enqueue(e.Value);
        }

        public string ReadCache()
        {
            var sb = new StringBuilder();
            while (_buffer.TryDequeue(out var c))
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        #region IDisposable Support

        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _keboardCache.OnChar -= KeyboardCache_OnCharEvent;
                }

                _disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);

        #endregion IDisposable Support
    }
}