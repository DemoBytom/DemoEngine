using System;
using System.Drawing;
using System.Windows.Forms;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Models.Options;
using Demo.Engine.Core.Notifications.Keyboard;
using Demo.Engine.Core.Platform;
using Demo.Engine.Platform.NetStandard.Win32.WindowMessage;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Demo.Engine.Windows.Platform.Netstandard.Win32
{
    public partial class RenderingForm : Form, IRenderingControl
    {
        private readonly FormWindowState _previousWindowState;
        private readonly FormSettings _formSettings;
        private Point _currentNonFullscreenPosition;
        private readonly bool _allowUserResizing;
        private readonly ILogger<RenderingForm> _logger;
        private readonly IMediator _mediator;

        public RenderingForm(
            ILogger<RenderingForm> logger,
            IOptions<FormSettings> formSettings,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
            _formSettings = formSettings.Value;
            _logger.LogInformation("Rendering form initialization {state}", "started");

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
                    Math.Min(screenBouds.Width, _formSettings.Width),
                    Math.Min(screenBouds.Height, _formSettings.Height));
                WindowState = FormWindowState.Normal;
                FormBorderStyle = _allowUserResizing
                    ? FormBorderStyle.Sizable
                    : FormBorderStyle.FixedToolWindow;
            }

            _logger.LogInformation("Rendering form initialization {state}", "completed");
        }

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
                            _mediator.Publish(new ClearKeysNotification());
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
                                _mediator.Publish(new KeyNotification(key, true));
                            }
                            break;
                        }
                    case WindowMessageTypes.SysKeyUp:
                    case WindowMessageTypes.KeyUp:
                        {
                            var key = (VirtualKeys)wparam;
                            _mediator.Publish(new KeyNotification(key, false));
                            break;
                        }
                    case WindowMessageTypes.Char:
                        {
                            var c = (char)wparam;
                            _mediator.Publish(new CharNotification(c));
                            break;
                        }
                }
            }

            base.WndProc(ref m);
        }
    }
}