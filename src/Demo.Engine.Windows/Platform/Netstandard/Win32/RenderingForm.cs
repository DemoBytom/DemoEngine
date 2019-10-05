using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Demo.Engine.Platform.NetStandard.Win32.WindowMessage;

namespace Demo.Engine.Windows.Platform.Netstandard.Win32
{
    internal partial class RenderingForm : Form, IRenderingForm
    {
        public RenderingForm()
        {
            InitializeComponent();
        }

        public bool DoEvents()
        {
            var isControlAlive = !IsDisposed;
            if (isControlAlive)
            {
                var localHandle = Handle;
                if (localHandle != IntPtr.Zero)
                {
                    // Previous code not compatible with Application.AddMessageFilter but faster then DoEvents
                    while (User32.PeekMessage(out _, IntPtr.Zero, 0, 0, 0) != 0)
                    {
                        if (User32.GetMessage(out var msg, IntPtr.Zero, 0, 0) == -1)
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                                "An error occured in main loop while processing windows messages. Error: {0}",
                                Marshal.GetLastWin32Error()));
                        }

                        if (msg.Msg == (int)WindowMessageTypes.Destroy)
                        {
                            isControlAlive = false;
                        }

                        var message = new Message() { HWnd = msg.HWnd, LParam = msg.LParam, Msg = msg.Msg, WParam = msg.WParam };
                        if (!Application.FilterMessage(ref message))
                        {
                            User32.TranslateMessage(ref msg);
                            User32.DispatchMessage(ref msg);
                        }
                    }
                }
            }
            return isControlAlive;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
        }
    }
}