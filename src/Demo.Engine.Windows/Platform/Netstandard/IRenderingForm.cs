using System;
using System.Windows.Forms;

namespace Demo.Engine.Windows.Platform.Netstandard
{
    internal interface IRenderingForm : IWin32Window, IDisposable
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

        /// <summary>
        /// Shows the form with the specified owner to the user.
        /// </summary>
        /// <param name="owner">
        /// Any object that implements System.Windows.Forms.IWin32Window and represents the top-level
        /// window that will own this form.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The form being shown is already visible.
        /// -or- The form specified in the owner parameter is the same as the form being shown. -or-
        /// The form being shown is disabled.
        /// -or- The form being shown is not a top-level window. -or- The form being shown as a dialog
        /// box is already a modal form. -or- The current process is not running in user interactive
        /// mode (for more information, see <see cref="SystemInformation.UserInteractive"/>).
        /// </exception>
        void Show(IWin32Window owner);
    }
}