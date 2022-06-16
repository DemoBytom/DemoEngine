// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.NetStandard.Win32.WindowMessage;

internal enum SizeValues
{
    /// <summary>
    /// The window has been resized, but neither the SIZE_MINIMIZED nor SIZE_MAXIMIZED value applies.
    /// </summary>
    SizeRestored = 0,

    /// <summary>
    /// The window has been minimized.
    /// </summary>
    SizeMinimized = 1,

    /// <summary>
    /// The window has been maximized.
    /// </summary>
    SizeMaximized = 2,

    /// <summary>
    /// Message is sent to all pop-up windows when some other window has been restored to its
    /// former size.
    /// </summary>
    SizeMaxShow = 3,

    /// <summary>
    /// Message is sent to all pop-up windows when some other window is maximized.
    /// </summary>
    SizeMaxHide = 4
}