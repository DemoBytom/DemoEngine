// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Drawing;
using Demo.Engine.Core.ValueObjects;

namespace Demo.Engine.Core.Interfaces.Platform;

public interface IRenderingControl : IDisposable
{
    /// <summary>
    /// Displays the control to the user.
    /// </summary>
    void Show();

    IntPtr Handle { get; }

    bool IsDisposed { get; }

    /// <summary>
    /// Width of the drawable area
    /// </summary>
    Width DrawWidth { get; }

    /// <summary>
    /// Height of the drawable area
    /// </summary>
    Height DrawHeight { get; }

    RectangleF DrawingArea { get; }
}