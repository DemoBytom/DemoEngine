// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Platform;
using FluentAssertions;
using Moq;
using Xunit;

namespace Demo.Engine.Core.UTs.Components.Keyboard;

public class KeyboardHandleTests
{
    private readonly MockRepository _mockRepository;

    private readonly Mock<IKeyboardCache> _mockKeyboardCache;
    private readonly Memory<bool> _keyboardCache;

    public KeyboardHandleTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);

        _mockKeyboardCache = _mockRepository.Create<IKeyboardCache>();
        _keyboardCache = Enumerable.Repeat(false, 256).ToArray().AsMemory();
        _mockKeyboardCache.SetupGet(o => o.KeysPressed).Returns(_keyboardCache);
    }

    private KeyboardHandle CreateKeyboardHandle() =>
        new(_mockKeyboardCache.Object);

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
        _mockRepository.VerifyAll();
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
        _mockRepository.VerifyAll();
    }
}