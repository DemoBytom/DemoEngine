using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Demo.Engine.Platform.NetStandard.Win32.WindowMessage;
using Demo.Engine.Windows.Models.Options;

namespace Demo.Engine.Windows.Platform.Netstandard.Win32
{
    internal partial class RenderingForm : Form, IRenderingForm
    {
        private readonly FormWindowState _previousWindowState;
        private readonly FormSettings _formSettings;
        private Point _currentNonFullscreenPosition;
        private readonly bool _allowUserResizing;

        public RenderingForm(FormSettings formSettings)
        {
            _formSettings = formSettings;
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            ResizeRedraw = true;

            _previousWindowState = FormWindowState.Normal;
            _currentNonFullscreenPosition = DesktopLocation;

            var fullscreen = _formSettings.Fullscreen;

            //Current screen
            var screen = Screen.FromControl(this);
            var screenBouds = screen.Bounds;

            if (fullscreen)
            {
                _currentNonFullscreenPosition = DesktopLocation;
                TopMost = true;
                _allowUserResizing = false;
                Location = new Point(0, 0);

                ClientSize = new Size(
                    screenBouds.Width,
                    screenBouds.Height);

                WindowState = FormWindowState.Maximized;
                FormBorderStyle = FormBorderStyle.None;
            }
            else
            {
                TopMost = false;
                _allowUserResizing = _formSettings.AllowResizing;
                DesktopLocation = new Point(
                    Math.Max(0, _currentNonFullscreenPosition.X),
                    Math.Max(0, _currentNonFullscreenPosition.Y));

                ClientSize = new Size(
                    Math.Min(screenBouds.Width, formSettings.Width),
                    Math.Min(screenBouds.Height, formSettings.Height));
                WindowState = FormWindowState.Normal;
                FormBorderStyle = _allowUserResizing
                    ? FormBorderStyle.Sizable
                    : FormBorderStyle.FixedToolWindow;
            }
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