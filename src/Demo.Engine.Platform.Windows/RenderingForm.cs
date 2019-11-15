using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Demo.Engine.Core.Models.Options;
using Demo.Engine.Core.Notifications.Keyboard;
using Demo.Engine.Core.Platform;
using Demo.Engine.Platform.NetStandard.Win32.WindowMessage;
using Demo.Engine.Platform.Windows;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Windows.Platform.Netstandard.Win32
{
    public partial class RenderingForm : Form, IRenderingForm
    {
        private readonly FormWindowState _previousWindowState;
        private readonly FormSettings _formSettings;
        private Point _currentNonFullscreenPosition;
        private readonly bool _allowUserResizing;
        private readonly ILogger<RenderingForm> _logger;
        private readonly IMediator _mediator;

        public RenderingForm(
            ILogger<RenderingForm> logger,
            FormSettings formSettings,
            IMediator mediator)
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

            _logger = logger;
            _mediator = mediator;
        }

        public bool DoEvents()
        {
            var isControlAlive = !IsDisposed;
            if (isControlAlive)
            {
                var localHandle = Handle;
                if (localHandle != IntPtr.Zero)
                {
                    // Previous code not compatible with Application.AddMessageFilter but faster
                    // then DoEvents
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
            var wparam = m.WParam.ToInt64();
            switch ((WindowMessageTypes)m.Msg)
            {
                //Not sure if I need it?
                case WindowMessageTypes.KillFocus:
                    {
                        _mediator.Publish(new ClearKeysNotification());
                        break;
                    }
                case WindowMessageTypes.KeyDown:
                    {
                        var key = (Keys)wparam;
                        //_logger.LogInformation("Pressed key: {key}", key);
                        //OnKeyDown(new EventArgs<char>((char)key));
                        _mediator.Publish(new KeyNotification((char)key, true));
                        //filter autorepeats
                        break;
                    }
                case WindowMessageTypes.KeyUp:
                    {
                        var key = (Keys)wparam;
                        //_logger.LogInformation("Released key: {key}", key);
                        _mediator.Publish(new KeyNotification((char)key, false));
                        break;
                    }
                case WindowMessageTypes.WM_CHAR:
                    {
                        var c = (char)wparam;
                        _mediator.Publish(new CharNotification(c));
                        //OnChar(c)
                        break;
                    }
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}