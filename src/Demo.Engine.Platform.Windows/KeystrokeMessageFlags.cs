// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Platform.Windows.MessageOperations;
using Demo.Engine.Platform.Windows.MessageOperations.Processors;

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
    internal const long KF_EXTENDED = 0x0100;

    /// <summary>
    /// Manipulates the dialog mode flag, which indicates whether a dialog box is active.
    /// Rarely used by modern applications.
    /// </summary>
    internal const long KF_DLGMODE = 0x0800;

    /// <summary>
    /// Manipulates the menu mode flag, which indicates whether a menu is active.
    /// Rarely used by modern applications.
    /// </summary>
    internal const long KF_MENUMODE = 0x1000;

    /// <summary>
    /// Manipulates the context code flag.
    /// </summary>
    internal const long KF_ALTDOWN = 0x2000;

    /// <summary>
    /// Manipulates the previous key state flag.
    /// </summary>
    internal const long KF_REPEAT = 0x4000;

    /// <summary>
    /// Manipulates the transition state flag
    /// </summary>
    internal const long KF_UP = 0x8000;

    extension(KeyOperation keyOperation)
    {
        private TReturn Process<TProcessor, TReturn>(
            TProcessor processor)
            where TProcessor : struct, IOperationProcessor<TReturn>, allows ref struct
            => processor.Process(in keyOperation.Message);

        /// <inheritdoc cref="KF_EXTENDED"/>
        internal bool IsExtended
            => keyOperation.Process<IsExtendedProcessor, bool>(default);

        /// <inheritdoc cref="KF_EXTENDED"/>
        internal bool IsNotExtended
            => keyOperation.Process<IsNotExtendedProcessor, bool>(default);

        /// <inheritdoc cref="KF_DLGMODE"/>
        internal bool IsDlgMode
            => keyOperation.Process<IsDlgModeProcessor, bool>(default);

        /// <inheritdoc cref="KF_DLGMODE"/>
        internal bool IsNotDlgMode
            => keyOperation.Process<IsNotDlgModeProcessor, bool>(default);

        /// <inheritdoc cref="KF_MENUMODE"/>
        internal bool IsMenuMode
            => keyOperation.Process<IsMenuModeProcessor, bool>(default);

        /// <inheritdoc cref="KF_MENUMODE"/>
        internal bool IsNotMenuMode
            => keyOperation.Process<IsNotMenuModeProcessor, bool>(default);

        /// <inheritdoc cref="KF_ALTDOWN"/>
        internal bool IsAltDown
            => keyOperation.Process<IsAltDownProcessor, bool>(default);

        /// <inheritdoc cref="KF_ALTDOWN"/>
        internal bool IsNotAltDown
            => keyOperation.Process<IsNotAltDownProcessor, bool>(default);

        /// <inheritdoc cref="KF_REPEAT"/>
        internal bool IsRepeat
            => keyOperation.Process<IsRepeatProcessor, bool>(default);

        /// <inheritdoc cref="KF_REPEAT"/>
        internal bool IsNotRepeat
            => keyOperation.Process<IsNotRepeatProcessor, bool>(default);

        /// <inheritdoc cref="KF_UP"/>
        internal bool IsUp
            => keyOperation.Process<IsUpProcessor, bool>(default);

        /// <inheritdoc cref="KF_UP"/>
        internal bool IsNotUp
            => keyOperation.Process<IsNotUpProcessor, bool>(default);
    }
}