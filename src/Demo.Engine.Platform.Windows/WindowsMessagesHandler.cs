// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.InteropServices;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Platform.Windows.WindowMessage;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Platform.Windows;

public class WindowsMessagesHandler : IOSMessageHandler
{
    private readonly ILogger<WindowsMessagesHandler> _logger;

    public WindowsMessagesHandler(
        ILogger<WindowsMessagesHandler> logger)
    {
        _logger = logger;

        User32.DisableProcessWindowsGhosting();
    }

    public unsafe bool DoEvents(IRenderingControl control)
    {
        var isControlAlive = !control.IsDisposed;
        if (isControlAlive)
        {
            //_ = User32.MsgWaitForMultipleObjectsEx(
            //    0,
            //    IntPtr.Zero,
            //    0,
            //    User32.QueueStatusFlags.AllInput,
            //    User32.WaitFlags.Alertable);

            var localHandle = control.Handle;
            if (localHandle != IntPtr.Zero)
            {
                NativeMessage msg;
                while (User32.PeekMessageW(&msg, hWnd: IntPtr.Zero, wMsgFilterMin: 0, wMsgFilterMax: 0, wRemoveMsg: 0))
                {
                    var getMessageW = User32.GetMessageW(&msg, hWnd: IntPtr.Zero, wMsgFilterMin: 0, wMsgFilterMax: 0);

                    if ((int)getMessageW == -1)
                    {
                        _logger.LogErroInMainLoopProcessingWindowsMessages(
                            Marshal.GetLastWin32Error);

                        return false;
                    }

                    if (msg.Message == (uint)WindowMessageTypes.NcDestroy)
                    {
                        isControlAlive = false;
                    }
                    if (msg.Message == (uint)WindowMessageTypes.Destroy)
                    {
                        isControlAlive = false;
                    }

                    var message = Message.Create(
                        hWnd: msg.HWnd,
                        msg: (int)(nint)msg.Message,
                        wparam: (nint)msg.WParam,
                        lparam: msg.LParam);

                    if (!Application.FilterMessage(ref message))
                    {
                        _ = User32.TranslateMessage(&msg);
                        _ = User32.DispatchMessageW(&msg);

#pragma warning disable CA1873 // Avoid potentially expensive logging
                        _logger.LogTrace("Dispatched message {MessageId} with Handle {MessageHandle} to controll {ControlHandle}",
                            message.Msg,
                            message.HWnd,
                            control.IsDisposed ? nint.Zero : control.Handle);
                        InspectWindow(message.HWnd);
#pragma warning restore CA1873 // Avoid potentially expensive logging
                    }
                    else
                    {

                    }
                }
            }
        }
        return isControlAlive;
    }

    public unsafe bool BlockingDoEvents(IRenderingControl control, CancellationToken cancellationToken)
    {
        var isControlAlive = !control.IsDisposed;
        if (isControlAlive)
        {
            var localHandle = control.Handle;
            if (localHandle != IntPtr.Zero)
            {
                NativeMessage msg;
                //while (User32.PeekMessageW(&msg, hWnd: IntPtr.Zero, wMsgFilterMin: 0, wMsgFilterMax: 0, wRemoveMsg: 0))
                while (User32.GetMessageW(&msg, hWnd: IntPtr.Zero, wMsgFilterMin: 0, wMsgFilterMax: 0) is var getMessageW && !cancellationToken.IsCancellationRequested)
                {
                    //var getMessageW = User32.GetMessageW(&msg, hWnd: IntPtr.Zero, wMsgFilterMin: 0, wMsgFilterMax: 0);

                    if ((int)getMessageW == -1)
                    {
                        _logger.LogErroInMainLoopProcessingWindowsMessages(
                            Marshal.GetLastWin32Error);

                        return false;
                    }

                    if (msg.Message == (uint)WindowMessageTypes.NcDestroy)
                    {
                        isControlAlive = false;
                    }
                    if (msg.Message == (uint)WindowMessageTypes.Destroy)
                    {
                        isControlAlive = false;
                        return false;
                    }
                    if (msg.Message == (uint)18)
                    {

                        return false;
                    }

                    var message = Message.Create(
                        hWnd: msg.HWnd,
                        msg: (int)(nint)msg.Message,
                        wparam: (nint)msg.WParam,
                        lparam: msg.LParam);

                    if (!Application.FilterMessage(ref message))
                    {
                        _ = User32.TranslateMessage(&msg);
                        _ = User32.DispatchMessageW(&msg);

#pragma warning disable CA1873 // Avoid potentially expensive logging
                        _logger.LogTrace("Dispatched message {MessageId} with Handle {MessageHandle} to controll {ControlHandle}",
                            message.Msg,
                            message.HWnd,
                            control.IsDisposed ? nint.Zero : control.Handle);
                        InspectWindow(message.HWnd);
#pragma warning restore CA1873 // Avoid potentially expensive logging
                    }
                    else
                    {

                    }
                }
            }
        }
        return isControlAlive
            && !cancellationToken.IsCancellationRequested;
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
}