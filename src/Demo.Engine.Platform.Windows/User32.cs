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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vortice.Mathematics;

[assembly: DisableRuntimeMarshalling]

namespace Demo.Engine.Platform.Windows;

internal static unsafe partial class User32
{
    public const string LIBRARY_NAME = "user32.dll";

    [LibraryImport("user32")]
    public static partial RawBool PeekMessageW(NativeMessage* lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [LibraryImport("user32")]
    public static partial RawBool GetMessageW(NativeMessage* lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [LibraryImport("user32")]
    public static partial RawBool TranslateMessage(NativeMessage* lpMsg);

    [LibraryImport("user32")]
    public static partial nint DispatchMessageW(NativeMessage* lpMsg);

    [LibraryImport("user32")]
    public static partial RawBool GetClientRect(nint hWnd, RawRect* lpRect);
}

[StructLayout(LayoutKind.Sequential)]
public partial struct NativeMessage
{
    public nint hwnd;
    public uint msg;
    public nuint wParam;
    public nint lParam;
    public uint time;
    public Int2 pt;
}

/// <summary>
/// Defines an integer rectangle (Left, Top, Right, Bottom)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct RawRect
{
    public RawRect(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    /// <summary>
    /// The left position.
    /// </summary>
    public readonly int Left;

    /// <summary>
    /// The top position.
    /// </summary>
    public readonly int Top;

    /// <summary>
    /// The right position
    /// </summary>
    public readonly int Right;

    /// <summary>
    /// The bottom position.
    /// </summary>
    public readonly int Bottom;

    /// <summary>
    /// Performs an implicit conversion from <see cre ="RawRect"/> to <see cref="RectI" />.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator RectI(in RawRect value) => RectI.FromLTRB(value.Left, value.Top, value.Right, value.Bottom);

    /// <summary>
    /// Performs an implicit conversion from <see cre ="RectI"/> to <see cref="RawRect" />.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The result of the conversion.</returns>
#pragma warning disable RCS1242 // Do not pass non-read-only struct by read-only reference
    public static implicit operator RawRect(in RectI value) => new(value.Left, value.Top, value.Right, value.Bottom);
#pragma warning restore RCS1242 // Do not pass non-read-only struct by read-only reference

    /// <summary>
    /// Performs an implicit conversion from <see cre ="RawRect"/> to <see cref="System.Drawing.Rectangle" />.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator System.Drawing.Rectangle(in RawRect value) => System.Drawing.Rectangle.FromLTRB(value.Left, value.Top, value.Right, value.Bottom);

    /// <summary>
    /// Performs an implicit conversion from <see cre ="System.Drawing.Rectangle"/> to <see cref="RawRect" />.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator RawRect(System.Drawing.Rectangle value) => new(value.Left, value.Top, value.Right, value.Bottom);

    //#if WINDOWS
    //    public static implicit operator RawRect(in global::Windows.Foundation.Rect value) => value.Left > int.MaxValue
    //            ? throw new ArgumentOutOfRangeException(nameof(value.Left))
    //            : value.Top > int.MaxValue
    //            ? throw new ArgumentOutOfRangeException(nameof(value.Top))
    //            : value.Right > int.MaxValue
    //            ? throw new ArgumentOutOfRangeException(nameof(value.Right))
    //            : value.Bottom > int.MaxValue
    //            ? throw new ArgumentOutOfRangeException(nameof(value.Bottom))
    //            : new((int)value.Left, (int)value.Top, (int)value.Right, (int)value.Bottom);
    //#endif
}

// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

/// <summary>
///     A boolean value stored on 4 bytes (instead of 1 in .NET).
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 4)]
public readonly struct RawBool : IEquatable<RawBool>
{
    private readonly int _boolValue;

    public RawBool(bool boolValue) => _boolValue = boolValue ? 1 : 0;

    public RawBool(int boolValue) => _boolValue = boolValue;

    public RawBool(uint boolValue) => _boolValue = (int)boolValue;

    public override bool Equals(object? obj)
        => obj switch
        {
            null => false,
            bool b => Equals(new RawBool(b)),
            RawBool rawBool => Equals(rawBool),
            int b => Equals(new RawBool(b)),
            uint b => Equals(new RawBool(b)),
            byte b => Equals(new RawBool(b)),
            sbyte b => Equals(new RawBool(b)),
            short b => Equals(new RawBool(b)),
            ushort b => Equals(new RawBool(b)),
            long b => Equals(new RawBool((int)b)),
            ulong b => Equals(new RawBool((uint)b)),
            _ => false
        };

    public bool Equals(RawBool other) => _boolValue != 0 == (other._boolValue != 0);

    public override int GetHashCode() => _boolValue;

    public static bool operator ==(RawBool left, RawBool right) => left.Equals(right);

    public static bool operator !=(RawBool left, RawBool right) => !left.Equals(right);

    public static implicit operator bool(RawBool booleanValue) => booleanValue._boolValue != 0;

    public static implicit operator RawBool(bool boolValue) => new(boolValue);

    public static explicit operator int(RawBool booleanValue) => booleanValue._boolValue;

    public static explicit operator RawBool(int boolValue) => new(boolValue);

    public static explicit operator uint(RawBool booleanValue) => (uint)booleanValue._boolValue;

    public static explicit operator RawBool(uint boolValue) => new(boolValue);

    public override string ToString() => (_boolValue != 0).ToString();
}