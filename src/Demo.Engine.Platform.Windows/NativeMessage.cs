// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.InteropServices;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.Windows;

[StructLayout(LayoutKind.Sequential)]
internal ref struct NativeMessage
{
    /// <summary>
    /// A handle to the window whose window procedure receives the message. This member is NULL when the message is a thread message.
    /// </summary>
    public nint HWnd;

    /// <summary>
    /// The message identifier. Applications can only use the low word; the high word is reserved by the system.
    /// </summary>
    public uint Message;

    /// <summary>
    /// Additional information about the message. The exact meaning depends on the value of the message member.
    /// </summary>
    public nuint WParam;

    /// <summary>
    /// Additional information about the message. The exact meaning depends on the value of the message member.
    /// </summary>
    public nint LParam;

    /// <summary>
    /// The time at which the message was posted.
    /// </summary>
    public uint Time;

    /// <summary>
    /// The cursor position, in screen coordinates, when the message was posted.
    /// </summary>
    public Int2 pt;
}