// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.Windows;

internal static class Helpers
{
    public static int MAKELONG(int low, int high)
        => (high << 16) | (low & 0xffff);

    public static IntPtr MAKELPARAM(int low, int high)
        => (high << 16) | (low & 0xffff);

    public static int HIWORD(int n)
        => (n >> 16) & 0xffff;

    public static int HIWORD(IntPtr n)
        => HIWORD(unchecked((int)(long)n));

    public static int LOWORD(int n)
        => n & 0xffff;

    public static int LOWORD(IntPtr n)
        => LOWORD(unchecked((int)(long)n));

    public static int SignedHIWORD(IntPtr n)
        => SignedHIWORD(unchecked((int)(long)n));

    public static int SignedLOWORD(IntPtr n)
        => SignedLOWORD(unchecked((int)(long)n));

    public static int SignedHIWORD(int n)
    {
        var i = (int)(short)((n >> 16) & 0xffff);
        return i;
    }

    public static int SignedLOWORD(int n)
    {
        var i = (int)(short)(n & 0xFFFF);
        return i;
    }
}