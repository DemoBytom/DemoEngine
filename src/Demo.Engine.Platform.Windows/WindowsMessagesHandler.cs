// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.InteropServices;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Maths.Interop;
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

                    var message = Message.Create(
                        hWnd: msg.HWnd,
                        msg: (int)(nint)msg.Message,
                        wparam: (nint)msg.WParam,
                        lparam: msg.LParam);

                    if (!Application.FilterMessage(ref message))
                    {
                        _ = User32.TranslateMessage(&msg);
                        _ = User32.DispatchMessageW(&msg);
                    }
                }
            }
        }
        return isControlAlive;
    }

    public unsafe bool BlockingDoEvents(IRenderingControl control, CancellationToken cancellationToken)
    {
        using var tokenRegistration = cancellationToken.Register(() => User32.PostQuitMessage(0));

        var isControlAlive = !control.IsDisposed;
        if (isControlAlive)
        {
            var localHandle = control.Handle;
            if (localHandle != IntPtr.Zero)
            {
                NativeMessage msg;
                RawBool getMessageW;

                while (getMessageW = User32.GetMessageW(&msg, hWnd: IntPtr.Zero, wMsgFilterMin: 0, wMsgFilterMax: 0) == true
                    /*cancellation token is required here in case an ESC is used to exit. 
                     * For now it doesn't seem to properly signal quit message? 
                     * Need to investigate why, because cancelling it should trigger User32.PostQuitMessage and end the pump..
                     * Maybe the StaThreadService is already down and cannot process it anymore? */
                    && !cancellationToken.IsCancellationRequested
                    )
                {
                    if ((int)getMessageW == -1)
                    {
                        _logger.LogErroInMainLoopProcessingWindowsMessages(
                            Marshal.GetLastWin32Error);

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
                    }
                }
            }
        }

        return isControlAlive
            && !cancellationToken.IsCancellationRequested
            ;
    }
}