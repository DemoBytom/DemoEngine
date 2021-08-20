// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.NetStandard.Win32.WindowMessage
{
    /// <summary>
    /// A window receives this message when the user chooses a command from the Window menu
    /// (formerly known as the system or control menu) or when the user chooses the maximize button,
    /// minimize button, restore button, or close button.
    /// </summary>
    /// <remarks>
    /// In WM_SYSCOMMAND messages, the four low-order bits of the wParam parameter are used
    /// internally by the system. To obtain the correct result when testing the value of wParam, an
    /// application must combine the value 0xFFF0 with the wParam value by using the bitwise AND operator.
    /// </remarks>
    internal enum SysCommandValues
    {
        /// <summary>
        /// SC_MONITORPOWER: Sets the state of the display. This command supports devices that have power-saving
        /// features, such as a battery-powered personal computer. The lParam parameter can have the
        /// following values:
        /// <list type="bullet">
        /// <item>-1 (the display is powering on)</item>
        /// <item>1 (the display is going to low power)</item>
        /// <item>2 (the display is being shut off)</item>
        /// </list>
        /// </summary>
        MonitorPower = 0xF170,

        /// <summary>
        /// SC_SCREENSAVE: Executes the screen saver application specified in the [boot] section of
        /// the System.ini file.
        /// </summary>
        ScreenSave = 0xF140
    }
}