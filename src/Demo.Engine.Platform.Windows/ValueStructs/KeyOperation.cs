// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.
using Demo.Engine.Core.Platform;
using static Demo.Engine.Platform.Windows.KeystrokeMessageFlags;

namespace Demo.Engine.Platform.Windows.ValueStructs;

internal readonly ref struct KeyOperation(
    in Message msg)
{
    private readonly ref readonly Message _msg = ref msg;

    /// <inheritdoc cref="KF_EXTENDED"/>
    internal bool IsExtended => _msg.LParam.IsExtended();

    /// <inheritdoc cref="KF_EXTENDED"/>
    internal bool IsNotExtended => _msg.LParam.IsNotExtended();

    /// <inheritdoc cref="KF_DLGMODE"/>
    internal bool IsDlgMode => _msg.LParam.IsDlgMode();

    /// <inheritdoc cref="KF_DLGMODE"/>
    internal bool IsNotDlgMode => _msg.LParam.IsNotDlgMode();

    /// <inheritdoc cref="KF_MENUMODE"/>
    internal bool IsMenuMode => _msg.LParam.IsMenuMode();

    /// <inheritdoc cref="KF_MENUMODE"/>
    internal bool IsNotMenuMode => _msg.LParam.IsNotMenuMode();

    /// <inheritdoc cref="KF_ALTDOWN"/>
    internal bool IsAltDown => _msg.LParam.IsAltDown();

    /// <inheritdoc cref="KF_ALTDOWN"/>
    internal bool IsNotAltDown => _msg.LParam.IsNotAltDown();

    /// <inheritdoc cref="KF_REPEAT"/>
    internal bool IsRepeat => _msg.LParam.IsRepeat();

    /// <inheritdoc cref="KF_REPEAT"/>
    internal bool IsNotRepeat => _msg.LParam.IsNotRepeat();

    /// <inheritdoc cref="KF_UP"/>
    internal bool IsUp => _msg.LParam.IsUp();

    /// <inheritdoc cref="KF_UP"/>
    internal bool IsNotUp => _msg.LParam.IsNotUp();

    public VirtualKeys Key => (VirtualKeys)_msg.WParam;
}