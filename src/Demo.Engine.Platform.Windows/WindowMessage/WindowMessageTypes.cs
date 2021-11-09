// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.NetStandard.Win32.WindowMessage;

internal enum WindowMessageTypes
{
    /// <summary>
    /// WM_SIZE: Sent to a window after its size has changed.
    /// </summary>
    Size = 0x0005,

    /// <summary>
    /// WM_KILLFOCUS: Sent to a window immediately before it loses the keyboard focus.
    /// </summary>
    KillFocus = 0x0008,

    /// <summary>
    /// WM_ACTIVATEAPP: Sent when a window belonging to a different application than the active
    /// window is about to be activated. The message is sent to the application whose window is
    /// being activated and to the application whose window is being deactivated.
    /// </summary>
    ActivateApp = 0x001C,

    /// <summary>
    /// The WM_DISPLAYCHANGE message is sent to all windows when the display resolution has changed.
    /// </summary>
    DisplayChange = 0x007E,

    /// <summary>
    /// WM_NCDESTROY: Notifies a window that its nonclient area is being destroyed. The DestroyWindow function sends the WM_NCDESTROY message to the window following the WM_DESTROY message.WM_DESTROY is used to free the allocated memory object associated with the window.
    ///<para>
    /// The WM_NCDESTROY message is sent after the child windows have been destroyed. In contrast, WM_DESTROY is sent before the child windows are destroyed.
    ///</para>
    /// </summary>
    Destroy = 0x0082,

    /// <summary>
    /// WM_KEYDOWN: Posted to the window with the keyboard focus when a nonsystem key is pressed.
    /// A nonsystem key is a key that is pressed when the ALT key is not pressed.
    /// </summary>
    KeyDown = 0x0100,

    /// <summary>
    /// WM_KEYUP: Posted to the window with the keyboard focus when a nonsystem key is released. A
    /// nonsystem key is a key that is pressed when the ALT key is not pressed, or a keyboard
    /// key that is pressed when a window has the keyboard focus.
    /// </summary>
    KeyUp = 0x0101,

    /// <summary>
    /// WM_CHAR: Posted to the window with the keyboard focus when a WM_KEYDOWN message is
    /// translated by the TranslateMessage function. The WM_CHAR message contains the character
    /// code of the key that was pressed.
    /// </summary>
    Char = 0x0102,

    /// <summary>
    /// WM_SYSKEYDOWN: Posted to the window with the keyboard focus when the user presses the
    /// F10 key (which activates the menu bar) or holds down the ALT key and then presses
    /// another key. It also occurs when no window currently has the keyboard focus; in this
    /// case, the WM_SYSKEYDOWN message is sent to the active window. The window that receives
    /// the message can distinguish between these two contexts by checking the context code in
    /// the lParam parameter.
    /// </summary>
    SysKeyDown = 0x0104,

    /// <summary>
    /// WM_SYSKEYUP: Posted to the window with the keyboard focus when the user releases a key
    /// that was pressed while the ALT key was held down. It also occurs when no window
    /// currently has the keyboard focus; in this case, the WM_SYSKEYUP message is sent to the
    /// active window. The window that receives the message can distinguish between these two
    /// contexts by checking the context code in the lParam parameter.
    /// </summary>
    SysKeyUp = 0x0105,

    /// <summary>
    /// WM_SYSCOMMAND: A window receives this message when the user chooses a command from the
    /// Window menu (formerly known as the system or control menu) or when the user chooses the
    /// maximize button, minimize button, restore button, or close button.
    /// </summary>
    SysCommand = 0x0112,

    /// <summary>
    /// WM_MENUCHAR: Sent when a menu is active and the user presses a key that does not
    /// correspond to any mnemonic or accelerator key. This message is sent to the window that
    /// owns the menu.
    /// </summary>
    MenuChar = 0x0120,

    /// <summary>
    /// WM_POWERBROADCAST: Notifies applications that a power-management event has occurred.
    /// </summary>
    PowerBroadcast = 0x0218
}