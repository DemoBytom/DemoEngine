using System;
using System.Drawing;
using System.Windows.Forms;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Models.Options;
using Demo.Engine.Core.Notifications.Keyboard;
using Demo.Engine.Core.Platform;
using Demo.Engine.Platform.NetStandard.Win32.WindowMessage;
using Demo.Tools.Common.Logging;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Demo.Engine.Windows.Platform.Netstandard.Win32
{
    public partial class RenderingForm : Form, IRenderingControl
    {
        //private readonly FormWindowState _previousWindowState;
        private readonly IOptionsMonitor<RenderSettings> _formSettings;

        private Point _currentNonFullscreenPosition;
        private readonly bool _allowUserResizing;

        //private readonly ILogger<RenderingForm> _logger;
        private readonly IMediator _mediator;

        public RenderingForm(
            ILogger<RenderingForm> logger,
            IOptionsMonitor<RenderSettings> formSettings,
            IMediator mediator)
        {
            using var loggingContext = logger.LogScopeInitialization();

            //_logger = logger;
            _mediator = mediator;
            _formSettings = formSettings;

            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            ResizeRedraw = true;

            //_previousWindowState = FormWindowState.Normal;
            _currentNonFullscreenPosition = DesktopLocation;

            var fullscreen = _formSettings.CurrentValue.Fullscreen;

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
                formSettings.CurrentValue.Width = ClientSize.Width;
                formSettings.CurrentValue.Height = ClientSize.Height;
            }
            else
            {
                TopMost = false;
                _allowUserResizing = _formSettings.CurrentValue.AllowResizing;
                DesktopLocation = new Point(
                    Math.Max(0, _currentNonFullscreenPosition.X),
                    Math.Max(0, _currentNonFullscreenPosition.Y));

                ClientSize = new Size(
                    Math.Min(screenBouds.Width, _formSettings.CurrentValue.Width),
                    Math.Min(screenBouds.Height, _formSettings.CurrentValue.Height));
                WindowState = FormWindowState.Normal;
                FormBorderStyle = _allowUserResizing
                    ? FormBorderStyle.Sizable
                    : FormBorderStyle.FixedToolWindow;
            }
        }

        /// <summary>
        /// Width of the drawable area
        /// </summary>
        public int DrawWidth => ClientRectangle.Width;

        /// <summary>
        /// Height of the drawable area
        /// </summary>
        public int DrawHeight => ClientRectangle.Height;

        public RectangleF DrawingArea => ClientRectangle;

        protected override void WndProc(ref Message m)
        {
            if (!IsDisposed && !Disposing)
            {
                //we run 64bit only
                var wparam = m.WParam.ToInt64();
                switch ((WindowMessageTypes)m.Msg)
                {
                    case WindowMessageTypes.KillFocus:
                    {
                        _mediator.Publish(new ClearKeysNotification()).GetAwaiter().GetResult();
                        break;
                    }
                    case WindowMessageTypes.SysKeyDown:
                    case WindowMessageTypes.KeyDown:
                    {
                        var lparam = m.LParam.ToInt64();

                        //bit 30
                        if ((lparam & 0x4000_0000) == 0)
                        {
                            var key = (VirtualKeys)wparam;
                            _mediator.Publish(new KeyNotification(key, true)).GetAwaiter().GetResult();
                        }
                        break;
                    }
                    case WindowMessageTypes.SysKeyUp:
                    case WindowMessageTypes.KeyUp:
                    {
                        var key = (VirtualKeys)wparam;
                        _mediator.Publish(new KeyNotification(key, false)).GetAwaiter().GetResult();
                        break;
                    }
                    case WindowMessageTypes.Char:
                    {
                        var c = (char)wparam;
                        _mediator.Publish(new CharNotification(c)).GetAwaiter().GetResult();
                        break;
                    }
                }
            }

            base.WndProc(ref m);
        }
    }
}