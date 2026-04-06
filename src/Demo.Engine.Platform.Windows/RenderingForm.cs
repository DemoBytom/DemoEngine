// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.ComponentModel;
using System.Diagnostics;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Models.Options;
using Demo.Engine.Core.Notifications.Keyboard;
using Demo.Engine.Core.Platform;
using Demo.Engine.Core.ValueObjects;
using Demo.Engine.Platform.Windows;
using Demo.Engine.Platform.Windows.WindowMessage;
using Demo.Tools.Common.Logging;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Demo.Engine.Windows.Platform.Netstandard.Win32;

public partial class RenderingForm : Form, IRenderingControl
{
    //private readonly FormWindowState _previousWindowState;
    private readonly IOptionsMonitor<RenderSettings> _formSettings;

    private Point _currentNonFullscreenPosition;
    private Size? _currentNonFullScreenSize;
    private FormWindowState _currentNonFullscreenState;
    private bool _allowUserResizing;

    private readonly ILogger<RenderingForm> _logger;
    private readonly IMediator _mediator;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsFullscreen { get; private set; }

    public RenderingForm(
        ILogger<RenderingForm> logger,
        IOptionsMonitor<RenderSettings> formSettings,
        IMediator mediator)
    {
        using var loggingContext = logger.LogScopeInitialization();

        _logger = logger;
        _mediator = mediator;
        _formSettings = formSettings;

        InitializeComponent();

        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        ResizeRedraw = true;

        _currentNonFullscreenPosition = DesktopLocation;
        _currentNonFullscreenState = FormWindowState.Normal;

        SetFullscreen(_formSettings.CurrentValue.Fullscreen);

        ResizeEnd += (sender, args) =>
        {
            if (!IsFullscreen)
            {
                //_formSettings.CurrentValue.Width = ClientSize.Width;
                //_formSettings.CurrentValue.Height = ClientSize.Height;
#pragma warning disable CA1873 // Avoid potentially expensive logging
                _logger.LogTrace(
                    "Resize ended, new size: {Width}x{Height}",
                    ClientSize.Width,
                    ClientSize.Height);
#pragma warning restore CA1873 // Avoid potentially expensive logging
            }
        };
    }

    public event EventHandler<RenderingControlSizeEventArgs>? UserResized;

    protected void OnUserResized(scoped in RenderingControlSizeEventArgs e)
        => UserResized?.Invoke(this, e);

    protected override void OnResizeEnd(EventArgs e)
    {
        base.OnResizeEnd(e);
        var width = new Width(ClientSize.Width);
        var height = new Height(ClientSize.Height);

        OnUserResized(new RenderingControlSizeEventArgs(
            ref width,
            ref height));
    }

    protected override void OnMaximizedBoundsChanged(EventArgs e)
        => base.OnMaximizedBoundsChanged(e);

    /// <summary>
    /// Width of the drawable area
    /// </summary>
    public Width DrawWidth => ClientRectangle.Width;

    /// <summary>
    /// Height of the drawable area
    /// </summary>
    public Height DrawHeight => ClientRectangle.Height;

    public RectangleF DrawingArea => ClientRectangle;

    public void SetFullscreen(
        bool fullscreen)
    {
        //Current screen
        var screen = Screen.FromControl(this);
        var screenBouds = screen.Bounds;

        if (fullscreen)
        {
            _currentNonFullscreenPosition = DesktopLocation;
            _currentNonFullScreenSize = ClientSize;
            _currentNonFullscreenState = WindowState;

            TopMost = true;
            _allowUserResizing = false;
            Location = new Point(screen.Bounds.X, screen.Bounds.Y);
            //new Point(0, 0);

            ClientSize = new Size(
                screenBouds.Width,
                screenBouds.Height);

            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.None;
            _formSettings.CurrentValue.Width = ClientSize.Width;
            _formSettings.CurrentValue.Height = ClientSize.Height;
        }
        else
        {
            TopMost = false;
            _allowUserResizing = _formSettings.CurrentValue.AllowResizing;
            DesktopLocation = new Point(
                Math.Max(0, _currentNonFullscreenPosition.X),
                Math.Max(0, _currentNonFullscreenPosition.Y));

            ClientSize = _currentNonFullScreenSize ?? new Size(
                Math.Min(screenBouds.Width, _formSettings.CurrentValue.Width),
                Math.Min(screenBouds.Height, _formSettings.CurrentValue.Height));
            WindowState = _currentNonFullscreenState;
            FormBorderStyle = _allowUserResizing
                ? FormBorderStyle.Sizable
                : FormBorderStyle.FixedToolWindow;

            _formSettings.CurrentValue.Width = ClientSize.Width;
            _formSettings.CurrentValue.Height = ClientSize.Height;
        }

        IsFullscreen = fullscreen;
    }

    private bool _resized = false;

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
                    _mediator.Publish(new ClearKeysNotification()).Preserve().GetAwaiter().GetResult();
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
                        _mediator.Publish(new KeyNotification(key, true)).Preserve().GetAwaiter().GetResult();
                    }
                    break;
                }
                case WindowMessageTypes.SysKeyUp:
                case WindowMessageTypes.KeyUp:
                {
                    var key = (VirtualKeys)wparam;
                    _mediator.Publish(new KeyNotification(key, false)).Preserve().GetAwaiter().GetResult();
                    break;
                }
                case WindowMessageTypes.Char:
                {
                    var c = (char)wparam;
                    _mediator.Publish(new CharNotification(c)).Preserve().GetAwaiter().GetResult();
                    break;
                }

                case WindowMessageTypes.Size:
                {
                    var lparam = m.LParam.ToInt32();

                    //LOWORD
                    var width = Helpers.LOWORD(lparam);

                    //HIWORD
                    var height = Helpers.HIWORD(lparam);

                    var resizingRequest = (SizeValues)m.WParam.ToInt64();
                    _resized = resizingRequest != SizeValues.SizeMinimized;

                    if (height != ClientSize.Height || width != ClientSize.Width)
                    {
                    }
                    Debug.WriteLine(
                        $"New: {width}x{height} vs {ClientSize.Width}x{ClientSize.Height}, requestType {resizingRequest}");
                    break;
                }
            }
        }

        if (_resized && User32.GetAsyncKeyState(VirtualKeys.LButton) >= 0)
        {
            Debug.WriteLine($"Resising ended");

            _resized = false;
        }

        base.WndProc(ref m);
    }
}