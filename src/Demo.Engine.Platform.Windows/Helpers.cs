// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.Windows;

internal static class Helpers
{
    public static int MAKELONG(int low, int high)
        => unchecked((int)(
            ((uint)(ushort)low) |
            ((uint)(ushort)high << 16)));

    public static nint MAKELPARAM(int low, int high)
        => MAKELONG(low, high);

    /// <summary>
    /// Extracts usigned high-order 16 bit word (bits 16-31)
    /// </summary>
    /// <param name="n"></param>
    /// <returns>high-order word (bits 16-31 of input)</returns>
    public static ushort HIWORD(int n)
        => (ushort)((uint)n >> 16);

    /// <summary>
    /// Extracts usigned high-order 16 bit word (bits 16-31)
    /// </summary>
    /// <param name="n"></param>
    /// <returns>high-order word (bits 16-31 of input)</returns>
    public static ushort HIWORD(nint n)
        => (ushort)((nuint)n >> 16);

    /// <summary>
    /// Extracts usigned low-order 16 bit word (bits 0-15)
    /// </summary>
    /// <param name="n"></param>
    /// <returns>low-order word (bits 0-15 of input)</returns>
    public static ushort LOWORD(int n)
        => (ushort)n;

    /// <summary>
    /// Extracts usigned low-order 16 bit word (bits 0-15)
    /// </summary>
    /// <param name="n"></param>
    /// <returns>low-order word (bits 0-15 of input)</returns>
    public static ushort LOWORD(nint n)
        => (ushort)(nuint)n;

    public static short SignedHIWORD(int n)
        => (short)((uint)n >> 16);

    public static short SignedHIWORD(nint n)
        => (short)((nuint)n >> 16);

    public static short SignedLOWORD(int n)
        => (short)n;

    public static short SignedLOWORD(nint n)
        => (short)(nuint)n;
}