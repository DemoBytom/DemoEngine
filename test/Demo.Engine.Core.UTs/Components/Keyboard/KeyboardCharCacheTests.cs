// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.
using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Components.Keyboard.Internal;
using Demo.Engine.Core.Interfaces.Components;
using TUnit.Assertions.Should;
using TUnit.Assertions.Should.Extensions;

namespace Demo.Engine.Core.UTs.Components.Keyboard;

public sealed class KeyboardCharCacheTests : IDisposable
{
    private readonly IKeyboardCache _keyboardCache;
    private readonly KeyboardCharCache _charResponse;
    public KeyboardCharCacheTests()
    {
        _keyboardCache = new KeyboardCache();
        _charResponse = new KeyboardCharCache(_keyboardCache);
    }

    [Test]
    public async Task ReadChars_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        _keyboardCache.Char('a');
        _keyboardCache.Char('l');
        _keyboardCache.Char('a');
        _keyboardCache.Char(' ');
        _keyboardCache.Char('m');
        _keyboardCache.Char('a');
        _keyboardCache.Char(' ');
        _keyboardCache.Char('k');
        _keyboardCache.Char('o');
        _keyboardCache.Char('t');
        _keyboardCache.Char('a');
        // Act
        var result = _charResponse.ReadCache();
        // Assert
        await result.Should().BeEqualTo("ala ma kota");
    }

    [Test]
    public async Task ReadChars_Dequeues_ProperlyAsync()
    {
        _keyboardCache.Char('a');
        _keyboardCache.Char('b');
        _keyboardCache.Char('c');
        var result1 = _charResponse.ReadCache();
        var result2 = _charResponse.ReadCache();
        await result1.Should().BeEqualTo("abc");
        await result2.Should().BeEmpty();
    }

    [Test]
    public async Task ReadChars_From_Multiple_HandlersAsync()
    {
        var charResponse1 = new KeyboardCharCache(_keyboardCache);
        var charResponse2 = new KeyboardCharCache(_keyboardCache);
        _keyboardCache.Char('a');
        _keyboardCache.Char('b');
        _keyboardCache.Char('c');
        await _charResponse.ReadCache().Should().BeEqualTo("abc");
        await charResponse1.ReadCache().Should().BeEqualTo("abc");
        await charResponse2.ReadCache().Should().BeEqualTo("abc");
    }

    [Test]
    public async Task ReadChars_Multiple_Handlers_Mixed_ReadsAsync()
    {
        var charResponse1 = new KeyboardCharCache(_keyboardCache);
        var charResponse2 = new KeyboardCharCache(_keyboardCache);
        _keyboardCache.Char('a');
        var charResponse1Read1 = charResponse1.ReadCache();
        _keyboardCache.Char('b');
        var charResponse2Read1 = charResponse2.ReadCache();
        _keyboardCache.Char('c');
        var charResponse3Read1 = _charResponse.ReadCache();
        var charResponse1Read2 = charResponse1.ReadCache();
        var charResponse2Read2 = charResponse2.ReadCache();
        var charResponse3Read2 = _charResponse.ReadCache();
        await charResponse1Read1.Should().BeEqualTo("a");
        await charResponse2Read1.Should().BeEqualTo("ab");
        await charResponse3Read1.Should().BeEqualTo("abc");
        await charResponse1Read2.Should().BeEqualTo("bc");
        await charResponse2Read2.Should().BeEqualTo("c");
        await charResponse3Read2.Should().BeEqualTo(string.Empty);
    }

    public void Dispose()
        => _charResponse.Dispose();
}