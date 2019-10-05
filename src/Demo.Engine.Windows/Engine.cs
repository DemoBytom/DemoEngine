using System;
using Demo.Engine.Windows.Platform.Netstandard;

namespace Demo.Engine.Windows
{
    internal class Engine : IDisposable
    {
        private readonly IRenderingForm _control;

        public Engine(
            IRenderingForm control)
        {
            _control = control;
        }

        internal void Run()
        {
            _control.Show();
            while (_control.DoEvents())
            {
            }
        }

        #region IDisposable Support

        /// <summary>
        /// To detect redundant calls
        /// </summary>
        private bool _disposedValue;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _control?.Dispose();
                }
                _disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose() =>
            Dispose(true);

        #endregion IDisposable Support
    }
}