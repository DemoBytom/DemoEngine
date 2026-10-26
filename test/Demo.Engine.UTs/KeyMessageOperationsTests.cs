// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Windows.Forms;
using Demo.Engine.Platform.Windows;
using TUnit.Assertions.Should;
using TUnit.Assertions.Should.Extensions;
using static Demo.Engine.Platform.Windows.KeystrokeMessageFlags;

namespace Demo.Engine.UTs;

public class KeyMessageOperationsTests
{
    [Test]
    [Arguments(KF_EXTENDED, true, DisplayName = nameof(KF_EXTENDED) + " true")]
    [Arguments(KF_DLGMODE, true, DisplayName = nameof(KF_DLGMODE) + " true")]
    [Arguments(KF_MENUMODE, true, DisplayName = nameof(KF_MENUMODE) + " true")]
    [Arguments(KF_ALTDOWN, true, DisplayName = nameof(KF_ALTDOWN) + " true")]
    [Arguments(KF_REPEAT, true, DisplayName = nameof(KF_REPEAT) + " true")]
    [Arguments(KF_UP, true, DisplayName = nameof(KF_UP) + " true")]
    [Arguments(KF_EXTENDED, false, DisplayName = nameof(KF_EXTENDED) + " false")]
    [Arguments(KF_DLGMODE, false, DisplayName = nameof(KF_DLGMODE) + " false")]
    [Arguments(KF_MENUMODE, false, DisplayName = nameof(KF_MENUMODE) + " false")]
    [Arguments(KF_ALTDOWN, false, DisplayName = nameof(KF_ALTDOWN) + " false")]
    [Arguments(KF_REPEAT, false, DisplayName = nameof(KF_REPEAT) + " false")]
    [Arguments(KF_UP, false, DisplayName = nameof(KF_UP) + " false")]
    internal async Task TestKeyMessageOperations(
        int lparamHigh,
        bool expected)
    {
        // Arrange
        var testMessage = Message.Create(
            hWnd: 0,
            msg: 0x0100, // WM_KEYDOWN
            wparam: 0x0D,
            lparam: Helpers.MAKELPARAM(
                low: 0000, high: lparamHigh)
            );

        // Act
        var keyOperation = testMessage.AsKeyOperation();

        var testValue = ((long)lparamHigh, expected) switch
        {
            (KF_EXTENDED, true) => keyOperation.IsExtended,
            (KF_EXTENDED, false) => keyOperation.IsNotExtended,

            (KF_DLGMODE, true) => keyOperation.IsDlgMode,
            (KF_DLGMODE, false) => keyOperation.IsNotDlgMode,

            (KF_MENUMODE, true) => keyOperation.IsMenuMode,
            (KF_MENUMODE, false) => keyOperation.IsNotMenuMode,

            (KF_ALTDOWN, true) => keyOperation.IsAltDown,
            (KF_ALTDOWN, false) => keyOperation.IsNotAltDown,

            (KF_REPEAT, true) => keyOperation.IsRepeat,
            (KF_REPEAT, false) => keyOperation.IsNotRepeat,

            (KF_UP, true) => keyOperation.IsUp,
            (KF_UP, false) => keyOperation.IsNotUp,

            _ => throw new NotSupportedException($"value {lparamHigh} is not supported!"),
        };

        // Assert
        await testValue.Should().BeEqualTo(expected);
    }
}