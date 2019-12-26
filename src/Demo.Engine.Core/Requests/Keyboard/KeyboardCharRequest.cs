using System;
using System.Text;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Tools.Common.Collections;
using Demo.Tools.Common.Sys;
using MediatR;

namespace Demo.Engine.Core.Requests.Keyboard
{
    public class KeyboardCharRequest : IRequest<KeyboardCharResponse>
    {
    }

    public class KeyboardCharResponse : IDisposable
    {
        private readonly CircularQueue<char> _buffer = new CircularQueue<char>(16);
        private readonly IKeyboardCache _keboardCache;

        public KeyboardCharResponse(IKeyboardCache keyboardCache)
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

        /// <summary>
        /// To detect redundant calls
        /// </summary>
        private bool _disposedValue = false;

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