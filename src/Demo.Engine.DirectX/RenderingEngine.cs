using System;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Platform;

namespace Demo.Engine.DirectX
{
    public class RenderingEngine : IRenderingEngine
    {
        private readonly IRenderingForm _renderingForm;

        public RenderingEngine(
            IRenderingForm renderingForm)
        {
            _renderingForm = renderingForm;
            _renderingForm.Show();
        }

        public bool DoEvents() => _renderingForm.DoEvents();

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RenderingEngine() { // Do not change this code. Put cleanup code in Dispose(bool
        // disposing) above. Dispose(false); }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above. GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}