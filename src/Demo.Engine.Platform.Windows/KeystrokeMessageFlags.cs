// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.Windows;

/// <summary>
/// The lParam parameter of a keystroke message contains additional information
/// about the keystroke that generated the message.
/// This information includes the repeat count, the scan code, the extended-key flag,
/// the context code, the previous key-state flag, and the transition-state flag.
/// </summary>
internal static class KeystrokeMessageFlags
{
    /// <summary>
    /// Manipulates the extended key flag.
    /// </summary>
    internal const long KF_EXTENDED = 0x0100_0000;

    /// <summary>
    /// Manipulates the dialog mode flag, which indicates whether a dialog box is active.
    /// Rarely used by modern applications.
    /// </summary>
    internal const long KF_DLGMODE = 0x0800_0000;

    /// <summary>
    /// Manipulates the menu mode flag, which indicates whether a menu is active.
    /// Rarely used by modern applications.
    /// </summary>
    internal const long KF_MENUMODE = 0x1000_0000;

    /// <summary>
    /// Manipulates the context code flag.
    /// </summary>
    internal const long KF_ALTDOWN = 0x2000_0000;

    /// <summary>
    /// Manipulates the previous key state flag.
    /// </summary>
    internal const long KF_REPEAT = 0x4000_0000;

    /// <summary>
    /// Manipulates the transition state flag
    /// </summary>
    internal const long KF_UP = 0x8000_0000;

    extension(long lparam)
    {
        /// <inheritdoc cref="KF_EXTENDED"/>
        internal bool IsExtended()
            => (lparam & KF_EXTENDED) != 0;

        /// <inheritdoc cref="KF_EXTENDED"/>
        internal bool IsNotExtended()
            => (lparam & KF_EXTENDED) == 0;

        /// <inheritdoc cref="KF_DLGMODE"/>
        internal bool IsDlgMode()
            => (lparam & KF_DLGMODE) != 0;

        /// <inheritdoc cref="KF_DLGMODE"/>
        internal bool IsNotDlgMode()
            => (lparam & KF_DLGMODE) == 0;

        /// <inheritdoc cref="KF_MENUMODE"/>
        internal bool IsMenuMode()
            => (lparam & KF_MENUMODE) != 0;

        /// <inheritdoc cref="KF_MENUMODE"/>
        internal bool IsNotMenuMode()
            => (lparam & KF_MENUMODE) == 0;

        /// <inheritdoc cref="KF_ALTDOWN"/>
        internal bool IsAltDown()
            => (lparam & KF_ALTDOWN) != 0;

        /// <inheritdoc cref="KF_ALTDOWN"/>
        internal bool IsNotAltDown()
            => (lparam & KF_ALTDOWN) == 0;

        /// <inheritdoc cref="KF_REPEAT"/>
        internal bool IsRepeat()
            => (lparam & KF_REPEAT) != 0;

        /// <inheritdoc cref="KF_REPEAT"/>
        internal bool IsNotRepeat()
            => (lparam & KF_REPEAT) == 0;

        /// <inheritdoc cref="KF_UP"/>
        internal bool IsUp()
            => (lparam & KF_UP) != 0;

        /// <inheritdoc cref="KF_UP"/>
        internal bool IsNotUp()
            => (lparam & KF_UP) == 0;
    }
}