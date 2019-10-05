namespace Demo.Engine.Platform.NetStandard.Win32.WindowMessage
{
    internal enum WindowMessageTypes
    {
        /// <summary>
        /// WM_SIZE: Sent to a window after its size has changed.
        /// </summary>
        Size = 0x0005,

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
        PowerBroadcast = 0x0218,

        /// <summary>
        /// WM_NCDESTROY: Notifies a window that its nonclient area is being destroyed. The DestroyWindow function sends the WM_NCDESTROY message to the window following the WM_DESTROY message.WM_DESTROY is used to free the allocated memory object associated with the window.
        ///<para>
        /// The WM_NCDESTROY message is sent after the child windows have been destroyed. In contrast, WM_DESTROY is sent before the child windows are destroyed.
        ///</para>
        /// </summary>
        Destroy = 0x0082
    }
}