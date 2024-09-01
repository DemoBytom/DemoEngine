// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.InteropServices;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Platform.NetStandard.Win32.WindowMessage;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Platform.Windows;

public class WindowsMessagesHandler : IOSMessageHandler
{
    private readonly ILogger<WindowsMessagesHandler> _logger;

    public WindowsMessagesHandler(
        ILogger<WindowsMessagesHandler> logger)
        => _logger = logger;

    public unsafe bool DoEvents(IRenderingControl control)
    {
        var isControlAlive = !control.IsDisposed;
        if (isControlAlive)
        {
            var localHandle = control.Handle;
            if (localHandle != IntPtr.Zero)
            {
                NativeMessage msg;
                while (User32.PeekMessageW(&msg, IntPtr.Zero, 0, 0, 0))
                {
                    var getMessageW = User32.GetMessageW(&msg, IntPtr.Zero, 0, 0);
                    if ((int)getMessageW == -1)
                    {
                        _logger.LogError(
                            "An error occured in main loop while processing windows messages. Error: {errorCode}",
                            Marshal.GetLastWin32Error());

                        return false;
                    }

                    if (msg.msg == (uint)WindowMessageTypes.Destroy)
                    {
                        isControlAlive = false;
                    }

                    //var message = new Message() { HWnd = msg.HWnd, LParam = msg.LParam, Msg = msg.Msg, WParam = msg.WParam };
                    var message = new Message
                    {
                        HWnd = msg.hwnd,
                        LParam = msg.lParam,
                        Msg = (int)msg.msg,
                        WParam = (nint)msg.wParam,
                    };

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
}