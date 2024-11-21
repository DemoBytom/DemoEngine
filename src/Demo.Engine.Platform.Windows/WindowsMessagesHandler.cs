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
        => _logger = logger;

    public bool DoEvents(IRenderingControl control)
    {
        var isControlAlive = !control.IsDisposed;
        if (isControlAlive)
        {
            var localHandle = control.Handle;
            if (localHandle != IntPtr.Zero)
            {
                while (User32.PeekMessage(out _, IntPtr.Zero, 0, 0, 0) != 0)
                {
                    if (User32.GetMessage(out var msg, IntPtr.Zero, 0, 0) == -1)
                    {
                        _logger.LogError(
                            "An error occured in main loop while processing windows messages. Error: {errorCode}",
                            Marshal.GetLastWin32Error());
                        return false;
                    }

                    if (msg.Msg == (int)WindowMessageTypes.Destroy)
                    {
                        isControlAlive = false;
                    }

                    //var message = new Message() { HWnd = msg.HWnd, LParam = msg.LParam, Msg = msg.Msg, WParam = msg.WParam };
                    if (!Application.FilterMessage(ref msg))
                    {
                        _ = User32.TranslateMessage(ref msg);
                        _ = User32.DispatchMessage(ref msg);
                    }
                }
            }
        }
        return isControlAlive;
    }
}