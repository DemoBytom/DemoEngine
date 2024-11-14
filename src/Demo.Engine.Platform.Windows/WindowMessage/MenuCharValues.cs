// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.Windows.WindowMessage;

/// <summary>
/// Sent when a menu is active and the user presses a key that does not correspond to any
/// mnemonic or accelerator key. This message is sent to the window that owns the menu.
/// </summary>
internal enum MenuCharValues
{
    /// <summary>
    /// Informs the system that it should discard the character the user pressed and create a
    /// short beep on the system speaker.
    /// </summary>
    Ignore = 0,

    /// <summary>
    /// Informs the system that it should close the active menu.
    /// </summary>
    Close = 1,

    /// <summary>
    /// Informs the system that it should choose the item specified in the low-order word of the
    /// return value. The owner window receives a WM_COMMAND message.
    /// </summary>
    Execute = 2,

    /// <summary>
    /// Informs the system that it should select the item specified in the low-order word of the
    /// return value.
    /// </summary>
    Select = 3
}