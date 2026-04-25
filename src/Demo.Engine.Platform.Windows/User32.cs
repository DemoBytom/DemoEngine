// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.InteropServices;
using Demo.Engine.Core.Maths.Interop;
using Demo.Engine.Core.Platform;
using Demo.Engine.Platform.Windows.WindowMessage;

namespace Demo.Engine.Platform.Windows;

internal static unsafe partial class User32
{
    public const string LIBRARY_NAME = "user32";

    [LibraryImport(LIBRARY_NAME, EntryPoint = "PeekMessageW")]
    public static partial RawBool PeekMessageW(NativeMessage* lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    /// <summary>
    /// Retrieves a message from the calling thread's message queue. The function dispatches incoming sent messages until a posted message is available for retrieval.
    /// <para/>
    /// Unlike GetMessage, the <see cref="PeekMessageW(NativeMessage*, nint, uint, uint, uint)"/> function does not wait for a message to be posted before returning.
    /// </summary>
    /// <param name="lpMsg">A pointer to an MSG structure that receives message information from the thread's message queue.</param>
    /// <param name="hWnd">
    /// A handle to the window whose messages are to be retrieved. The window must belong to the current thread.
    /// <para/>
    /// If hWnd is NULL, GetMessage retrieves messages for any window that belongs to the current thread, and any messages on the current thread's message queue whose hwnd value is NULL (see the MSG structure). Therefore if hWnd is NULL, both window messages and thread messages are processed.
    /// <para/>
    /// If hWnd is -1, GetMessage retrieves only messages on the current thread's message queue whose hwnd value is NULL, that is, thread messages as posted by PostMessage (when the hWnd parameter is NULL) or PostThreadMessage.
    /// </param>
    /// <param name="wMsgFilterMin">
    /// The integer value of the lowest message value to be retrieved. Use WM_KEYFIRST (0x0100) to specify the first keyboard message or WM_MOUSEFIRST (0x0200) to specify the first mouse message.
    /// <para/>
    /// Use WM_INPUT here and in wMsgFilterMax to specify only the WM_INPUT messages.
    /// <para/>
    /// If wMsgFilterMin and wMsgFilterMax are both zero, GetMessage returns all available messages(that is, no range filtering is performed).
    /// </param>
    /// <param name="wMsgFilterMax">
    /// The integer value of the highest message value to be retrieved. Use WM_KEYLAST to specify the last keyboard message or WM_MOUSELAST to specify the last mouse message.
    ///<para/>
    /// Use WM_INPUT here and in wMsgFilterMin to specify only the WM_INPUT messages.
    /// <para/>
    /// If wMsgFilterMin and wMsgFilterMax are both zero, GetMessage returns all available messages(that is, no range filtering is performed).
    /// </param>
    /// <returns>
    /// If the function retrieves a message other than <see cref="WindowMessageTypes.Quit"/>, the return value is nonzero.
    /// <para/>
    /// If the function retrieves the <see cref="WindowMessageTypes.Quit"/> message, the return value is zero.
    /// <para/>
    /// If there is an error, the return value is -1. For example, the function fails if hWnd is an invalid window handle or lpMsg is an invalid pointer.
    /// To get extended error information, call GetLastError.
    /// </returns>
    [LibraryImport(LIBRARY_NAME)]
    public static partial RawBool GetMessageW(NativeMessage* lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [LibraryImport(LIBRARY_NAME)]
    public static partial RawBool TranslateMessage(NativeMessage* lpMsg);

    [LibraryImport(LIBRARY_NAME)]
    public static partial nint DispatchMessageW(NativeMessage* lpMsg);

    [LibraryImport(LIBRARY_NAME)]
    public static partial RawBool GetClientRect(nint hWnd, RawRectangle* lpRect);

    [LibraryImport(LIBRARY_NAME)]
    public static partial short GetAsyncKeyState(VirtualKeys vKey);

    [LibraryImport(LIBRARY_NAME)]
    public static partial IntPtr GetForegroundWindow();

    [LibraryImport(LIBRARY_NAME, SetLastError = false)]
    internal static partial void PostQuitMessage(int nExitCode);

    /* debugging */
    [LibraryImport(LIBRARY_NAME, EntryPoint = "GetClassNameW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int GetClassName(
        IntPtr hWnd,
        Span<char> lpClassName,
        int nMaxCount);

    [LibraryImport(LIBRARY_NAME, EntryPoint = "GetWindowTextW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int GetWindowText(
        IntPtr hWnd,
        Span<char> lpString,
        int nMaxCount);

    [LibraryImport(LIBRARY_NAME)]
    public static partial uint GetWindowThreadProcessId(
        IntPtr hWnd,
        out uint processId);

    [LibraryImport(LIBRARY_NAME)]
    public static partial void DisableProcessWindowsGhosting();

    [Flags]
    public enum QueueStatusFlags : uint
    {
        AllInput = 0x04FF
    }

    [Flags]
    public enum WaitFlags : uint
    {
        None = 0,
        Alertable = 0x0002,
        AllInput = 0x0004
    }

    [LibraryImport(LIBRARY_NAME, SetLastError = true)]
    public static partial uint MsgWaitForMultipleObjectsEx(
        uint nCount,
        IntPtr pHandles, // Use IntPtr.Zero if waiting on 0 handles
        uint dwMilliseconds,
        QueueStatusFlags dwWakeMask,
        WaitFlags dwFlags);
}