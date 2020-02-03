using System;

namespace Demo.Engine.Core.Platform
{
    public interface IRenderingForm : IDisposable
    {
        /// <summary>
        /// Call this on each tick of the main loop to process all Windows messages in the queue
        /// </summary>
        /// <returns>if the Tick is successful</returns>
        /// <exception cref="InvalidOperationException">An error occured</exception>
        bool DoEvents();

        /// <summary>
        /// Displays the control to the user.
        /// </summary>
        void Show();
    }
}