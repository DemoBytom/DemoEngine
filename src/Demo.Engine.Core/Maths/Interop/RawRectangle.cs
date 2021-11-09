// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Demo.Engine.Windows.Maths.Interop
{
    [DebuggerDisplay("Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom}")]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct RawRectangle
    {
        /// <summary>
        /// Specifies the x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int Left;

        /// <summary>
        /// Specifies the y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int Top;

        /// <summary>
        /// Specifies the x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int Right;

        /// <summary>
        /// Specifies the y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int Bottom;

        /// <summary>
        /// The <see cref="RawRectangle"/> structure defines a rectangle by the coordinates of its
        /// upper-left and lower-right corners.
        /// </summary>
        /// <param name="left">x-coordinate of the upper-left corner of the rectangle</param>
        /// <param name="top">y-coordinate of the upper-left corner of the rectangle</param>
        /// <param name="right">x-coordinate of the lower-right corner of the rectangle</param>
        /// <param name="bottom">y-coordinate of the lower-right corner of the rectangle</param>
        public RawRectangle(
            int left,
            int top,
            int right,
            int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public bool IsEmpty =>
            Left == 0 &&
            Right == 0 &&
            Top == 0 &&
            Bottom == 0;
    }
}