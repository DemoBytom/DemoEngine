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
                        _logger.LogError(
                            "An error occured in main loop while processing windows messages. Error: {errorCode}",
                            Marshal.GetLastWin32Error());

                        return false;
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
                    }
                }
            }
        }
        return isControlAlive;
    }
}