// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.InteropServices;
using Demo.Engine.Windows.Maths.Interop;

namespace Demo.Engine.Platform.Windows;

internal static class User32
{
    public const string LIBRARY_NAME = "user32.dll";

    [DllImport(LIBRARY_NAME, EntryPoint = "PeekMessage")]
    public static extern int PeekMessage(out Message lpMsg, IntPtr hWnd, int wMsgFilterMin,
                                         int wMsgFilterMax, int wRemoveMsg);

    [DllImport(LIBRARY_NAME, EntryPoint = "GetMessage")]
    public static extern int GetMessage(out Message lpMsg, IntPtr hWnd, int wMsgFilterMin,
                                         int wMsgFilterMax);

    [DllImport(LIBRARY_NAME, EntryPoint = "TranslateMessage")]
    public static extern int TranslateMessage(ref Message lpMsg);

    [DllImport(LIBRARY_NAME, EntryPoint = "DispatchMessage")]
    public static extern int DispatchMessage(ref Message lpMsg);

    [DllImport(LIBRARY_NAME, EntryPoint = "GetClientRect")]
    public static extern bool GetClientRect(IntPtr hWnd, out RawRectangle lpRect);
}