// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Platform;
using TUnit.Assertions.Should;
using TUnit.Assertions.Should.Extensions;

namespace Demo.Engine.Core.UTs.Components.Keyboard;

public class KeyboardHandleTests
{
    private readonly Mock<IKeyboardCache> _mockKeyboardCache;
    private readonly Memory<bool> _keyboardCache;

    public KeyboardHandleTests()
    {
        _mockKeyboardCache = IKeyboardCache.Mock(MockBehavior.Strict);
        _keyboardCache = Enumerable.Repeat(false, 256).ToArray().AsMemory();
        _ = _mockKeyboardCache.KeysPressed.Returns(_keyboardCache);
    }

    private KeyboardHandle CreateKeyboardHandle()
        => new(_mockKeyboardCache.Object);

    [Test]
    public async Task GetKeyPressed_Only_One_Pressed()
    {
        // Arrange
        var keyboardHandle = CreateKeyboardHandle();
        const VirtualKeys TESTKEY = VirtualKeys.Q;

        _keyboardCache.Span[(char)TESTKEY] = true;

        // Act
        foreach (var key in Enum.GetValues<VirtualKeys>().Cast<VirtualKeys>())
        {
            var result = keyboardHandle.GetKeyPressed(key);
            await result.Should().BeEqualTo(key == TESTKEY, $"{key} is {result}");
        }

        // Assert
        _mockKeyboardCache.KeysPressed.WasCalled(Times.Exactly(189));
    }

    [Test]
    public async Task GetKeyPressed_Multiple_Keys_PressedAsync()
    {
        // Arrange
        var keyboardHandle = CreateKeyboardHandle();
        var testKeys = new[]
        {
            VirtualKeys.Q,
            VirtualKeys.W,
            VirtualKeys.ShiftKey
        };
        foreach (var key in testKeys)
        {
            _keyboardCache.Span[(char)key] = true;
        }

        // Act
        foreach (var key in Enum.GetValues<VirtualKeys>().Cast<VirtualKeys>())
        {
            var result = keyboardHandle.GetKeyPressed(key);
            await result.Should().BeEqualTo(testKeys.Contains(key), $"{key} is {result}");
        }

        // Assert
        _mockKeyboardCache.KeysPressed.WasCalled(Times.Exactly(189));
    }
}