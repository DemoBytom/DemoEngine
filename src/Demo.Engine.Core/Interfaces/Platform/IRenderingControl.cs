using System;

namespace Demo.Engine.Core.Interfaces.Platform
{
    public interface IRenderingControl : IDisposable
    {
        /// <summary>
        /// Displays the control to the user.
        /// </summary>
        void Show();

        IntPtr Handle { get; }

        bool IsDisposed { get; }
    }
}