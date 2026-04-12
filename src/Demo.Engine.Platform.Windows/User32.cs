// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

//using System.Runtime.InteropServices;
//using Demo.Engine.Windows.Maths.Interop;

//namespace Demo.Engine.Platform.Windows;

//internal static class User32
//{
//    public const string LIBRARY_NAME = "user32.dll";

//    [DllImport(LIBRARY_NAME, EntryPoint = "PeekMessage")]
//    public static extern int PeekMessage(out Message lpMsg, IntPtr hWnd, int wMsgFilterMin,
//                                         int wMsgFilterMax, int wRemoveMsg);

//    [DllImport(LIBRARY_NAME, EntryPoint = "GetMessage")]
//    public static extern int GetMessage(out Message lpMsg, IntPtr hWnd, int wMsgFilterMin,
//                                         int wMsgFilterMax);

//    [DllImport(LIBRARY_NAME, EntryPoint = "TranslateMessage")]
//    public static extern int TranslateMessage(ref Message lpMsg);

//    [DllImport(LIBRARY_NAME, EntryPoint = "DispatchMessage")]
//    public static extern int DispatchMessage(ref Message lpMsg);

//    [DllImport(LIBRARY_NAME, EntryPoint = "GetClientRect")]
//    public static extern bool GetClientRect(IntPtr hWnd, out RawRectangle lpRect);
//}

using System.Runtime.InteropServices;
using Demo.Engine.Core.Maths.Interop;
using Demo.Engine.Core.Platform;

namespace Demo.Engine.Platform.Windows;

internal static unsafe partial class User32
{
    public const string LIBRARY_NAME = "user32.dll";

    [LibraryImport("user32", EntryPoint = "PeekMessageW")]
    public static partial RawBool PeekMessageW(NativeMessage* lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [LibraryImport("user32")]
    public static partial RawBool GetMessageW(NativeMessage* lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [LibraryImport("user32")]
    public static partial RawBool TranslateMessage(NativeMessage* lpMsg);

    [LibraryImport("user32")]
    public static partial nint DispatchMessageW(NativeMessage* lpMsg);

    [LibraryImport("user32")]
    public static partial RawBool GetClientRect(nint hWnd, RawRectangle* lpRect);

    [LibraryImport("user32")]
    public static partial short GetAsyncKeyState(VirtualKeys vKey);

    [LibraryImport("user32")]
    public static partial IntPtr GetForegroundWindow();

    /* debugging */
    [LibraryImport("user32", EntryPoint = "GetClassNameW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int GetClassName(
        IntPtr hWnd,
        Span<char> lpClassName,
        int nMaxCount);

    [LibraryImport("user32", EntryPoint = "GetWindowTextW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int GetWindowText(
        IntPtr hWnd,
        Span<char> lpString,
        int nMaxCount);

    [LibraryImport("user32")]
    public static partial uint GetWindowThreadProcessId(
        IntPtr hWnd,
        out uint processId);

    [LibraryImport("user32")]
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

    [LibraryImport("user32", SetLastError = true)]
    public static partial uint MsgWaitForMultipleObjectsEx(
        uint nCount,
        IntPtr pHandles, // Use IntPtr.Zero if waiting on 0 handles
        uint dwMilliseconds,
        QueueStatusFlags dwWakeMask,
        WaitFlags dwFlags);

    [LibraryImport("user32", SetLastError = false)]
    internal static partial void PostQuitMessage(int nExitCode);
}