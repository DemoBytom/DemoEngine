// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.ComponentModel;
using System.Diagnostics;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Maths.Interop;
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
    private FormWindowState _previousWindowState;
    private Size _cachedSize;
    private readonly IOptionsMonitor<RenderSettings> _formSettings;

    private Point _currentNonFullscreenPosition;
    private Size? _currentNonFullScreenSize;
    private FormWindowState _currentNonFullscreenState;
    private bool _allowUserResizing;

    private bool _isUserResizing;
    private bool _isSizeChangedWithoutResizeBegin;

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

        Name = "RenderingForm";
        Text = "Demo Engine";

        InitializeComponent();

        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        ResizeRedraw = true;

        _currentNonFullscreenPosition = DesktopLocation;
        _currentNonFullscreenState = FormWindowState.Normal;
        _previousWindowState = FormWindowState.Normal;

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
                : FormBorderStyle.FixedSingle;

            _formSettings.CurrentValue.Width = ClientSize.Width;
            _formSettings.CurrentValue.Height = ClientSize.Height;
        }

        IsFullscreen = fullscreen;

        FormClosed += (s, e) => OnRenderingFormClosed(e);
    }

    void InspectWindow(IntPtr hwnd)
    {
        Span<char> classBuffer = stackalloc char[256];
        var classLen = User32.GetClassName(hwnd, classBuffer, classBuffer.Length);
        var className = new string(classBuffer.Slice(0, classLen));

        Span<char> textBuffer = stackalloc char[256];
        var textLen = User32.GetWindowText(hwnd, textBuffer, textBuffer.Length);
        var title = new string(textBuffer[..textLen]);

        var tid = User32.GetWindowThreadProcessId(hwnd, out var pid);

#pragma warning disable CA1873 // Avoid potentially expensive logging
#pragma warning disable CA1727 // Use PascalCase for named placeholders
        _logger.LogTrace("HWND: {hwnd} Class: {className} Title: {title} PID: {pid} TID: {tid}",
            hwnd, className, title, pid, tid);
#pragma warning restore CA1727 // Use PascalCase for named placeholders
#pragma warning restore CA1873 // Avoid potentially expensive logging
    }

    protected override unsafe void WndProc(ref Message m)
    {
#pragma warning disable CA1873 // Avoid potentially expensive logging
        _logger.LogTrace(
            "Received window message: {Message}",
            (WindowMessageTypes)m.Msg);
#pragma warning restore CA1873 // Avoid potentially expensive logging

        if (!IsDisposed && !Disposing)
        {
            //we run 64bit only
            var wparam = m.WParam.ToInt64();
            switch ((WindowMessageTypes)m.Msg)
            {
                case WindowMessageTypes.Destroy:
                    break;
                case WindowMessageTypes.NcDestroy:
                    break;
                case (WindowMessageTypes)0x0112: // WM_SYSCOMMAND
                                                 // 0xF020 is SC_MINIMIZE, 0xF120 is SC_RESTORE
                    var command = (int)m.WParam & 0xFFF0;
                    if (command == 0xF060) //SC CLOSE
                    {
                        //Close();
                        //User32.PostQuitMessage(0);
                        //return;
                    }
#pragma warning disable CA1873 // Avoid potentially expensive logging
                    _logger.LogInformation("Processing SysCommand: {Command}", command);
#pragma warning restore CA1873 // Avoid potentially expensive logging
                    break;

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
                    var width = new Width(Helpers.LOWORD(lparam));

                    //HIWORD
                    var height = new Height(Helpers.HIWORD(lparam));

                    var resizingRequest = (SizeValues)m.WParam.ToInt64();

                    if (height != ClientSize.Height || width != ClientSize.Width)
                    {
                    }

                    /***/
                    if (resizingRequest == SizeValues.SizeMinimized)
                    {
                        _previousWindowState = FormWindowState.Minimized;
                        OnPauseRendering(EventArgs.Empty);
                        _logger.LogInformation("Window minimized, pausing rendering");
                    }
                    else
                    {
                        RawRectangle clientRect;
                        _ = User32.GetClientRect(Handle, &clientRect);
                        if (clientRect.Bottom - clientRect.Top == 0)
                        {
                            // Rapidly clicking the task bar to minimize and restore a window
                            // can cause a WM_SIZE message with SIZE_RESTORED when 
                            // the window has actually become minimized due to rapid change
                            // so just ignore this message
                        }
                        else if (resizingRequest == SizeValues.SizeMaximized)
                        {
                            if (_previousWindowState == FormWindowState.Minimized)
                            {
                                OnResumeRendering(EventArgs.Empty);
                            }

                            _previousWindowState = FormWindowState.Maximized;

                            OnUserResized(new RenderingControlSizeEventArgs(
                                ref width,
                                ref height));

                            _cachedSize = Size;
                        }
                        else if (resizingRequest == SizeValues.SizeRestored)
                        {
                            if (_previousWindowState == FormWindowState.Minimized)
                            {
                                OnResumeRendering(EventArgs.Empty);
                                _logger.LogInformation("Window restored from minimized state, resuming rendering");
                            }

                            if (!_isUserResizing && (Size != _cachedSize || _previousWindowState == FormWindowState.Maximized))
                            {
                                _previousWindowState = FormWindowState.Normal;

                                if (_cachedSize != Size.Empty)
                                {
                                    _isSizeChangedWithoutResizeBegin = true;
                                }
                            }
                            else
                            {
                                _previousWindowState = FormWindowState.Normal;
                            }
                        }
                    }
                    /***/
                    Debug.WriteLine(
                        $"New: {width}x{height} vs {ClientSize.Width}x{ClientSize.Height}, requestType {resizingRequest}");
                    break;
                }
            }
        }

        base.WndProc(ref m);
    }

    protected override void OnResizeBegin(EventArgs e)
    {
        _isUserResizing = true;

        base.OnResizeBegin(e);
        _cachedSize = Size;
        OnPauseRendering(e);
    }

    protected override void OnResizeEnd(EventArgs e)
    {
        base.OnResizeEnd(e);
        if (_isUserResizing && _cachedSize != Size)
        {
            var width = new Width(ClientSize.Width);
            var height = new Height(ClientSize.Height);

            OnUserResized(new RenderingControlSizeEventArgs(
                ref width,
                ref height));
        }

        _isUserResizing = false;
        OnResumeRendering(e);
    }

    protected override void OnClientSizeChanged(EventArgs e)
    {
        base.OnClientSizeChanged(e);

        if (!_isUserResizing && (_isSizeChangedWithoutResizeBegin || _cachedSize != Size))
        {
            _isSizeChangedWithoutResizeBegin = false;
            _cachedSize = Size;

            var width = new Width(ClientSize.Width);
            var height = new Height(ClientSize.Height);

            OnUserResized(new RenderingControlSizeEventArgs(
                ref width,
                ref height));
        }
    }

    public event EventHandler<RenderingControlSizeEventArgs>? UserResized;

    protected virtual void OnUserResized(scoped in RenderingControlSizeEventArgs e)
        => UserResized?.Invoke(this, e);

    protected virtual void OnPauseRendering(EventArgs eventArgs)
    {
        // TODO
    }

    protected virtual void OnResumeRendering(EventArgs eventArgs)
    {
        // TODO
    }

    public event EventHandler<EventArgs>? RenderingFormClosed;

    protected virtual void OnRenderingFormClosed(FormClosedEventArgs eventArgs)
        => RenderingFormClosed?.Invoke(this, eventArgs);

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        if (disposing)
        {
            FormClosed -= (s, e) => OnRenderingFormClosed(e);
        }
        base.Dispose(disposing);
    }
}