// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Platform;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Demo.Engine.Core.UTs.Components.Keyboard;

public class KeyboardHandleTests
{
    private readonly IKeyboardCache _mockKeyboardCache;
    private readonly Memory<bool> _keyboardCache;

    public KeyboardHandleTests()
    {
        _mockKeyboardCache = Substitute.For<IKeyboardCache>();
        _keyboardCache = Enumerable.Repeat(false, 256).ToArray().AsMemory();
        _mockKeyboardCache.KeysPressed.Returns(_keyboardCache);
    }

    private KeyboardHandle CreateKeyboardHandle() =>
        new KeyboardHandle(_mockKeyboardCache);

    [Fact]
    public void GetKeyPressed_Only_One_Pressed()
    {
        // Arrange
        var keyboardHandle = CreateKeyboardHandle();
        const VirtualKeys TESTKEY = VirtualKeys.Q;

        _keyboardCache.Span[(char)TESTKEY] = true;

        // Act
        foreach (var key in Enum.GetValues(typeof(VirtualKeys)).Cast<VirtualKeys>())
        {
            var result = keyboardHandle.GetKeyPressed(key);
            result.Should().Be(key == TESTKEY, $"{key} is {result}");
        }

        // Assert
        _ = _mockKeyboardCache.Received().KeysPressed;
    }

    [Fact]
    public void GetKeyPressed_Multiple_Keys_Pressed()
    {
        // Arrange
        var keyboardHandle = CreateKeyboardHandle();
        var testKeys = new[]{
                VirtualKeys.Q,
                VirtualKeys.W,
                VirtualKeys.ShiftKey
            };

        foreach (var key in testKeys)
        {
            _keyboardCache.Span[(char)key] = true;
        }

        // Act
        foreach (var key in Enum.GetValues(typeof(VirtualKeys)).Cast<VirtualKeys>())
        {
            var result = keyboardHandle.GetKeyPressed(key);
            result.Should().Be(testKeys.Contains(key), $"{key} is {result}");
        }

        // Assert
        _ = _mockKeyboardCache.Received().KeysPressed;
    }
}