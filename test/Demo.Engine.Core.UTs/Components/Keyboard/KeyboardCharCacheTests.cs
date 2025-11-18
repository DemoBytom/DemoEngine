// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.
using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Components.Keyboard.Internal;
using Demo.Engine.Core.Interfaces.Components;
using Shouldly;

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
    public void ReadChars_StateUnderTest_ExpectedBehavior()
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
        result.ShouldBe("ala ma kota");
    }

    [Test]
    public void ReadChars_Dequeues_Properly()
    {
        _keyboardCache.Char('a');
        _keyboardCache.Char('b');
        _keyboardCache.Char('c');
        var result1 = _charResponse.ReadCache();
        var result2 = _charResponse.ReadCache();
        result1.ShouldBe("abc");
        result2.ShouldBeEmpty();
    }

    [Test]
    public void ReadChars_From_Multiple_Handlers()
    {
        var charResponse1 = new KeyboardCharCache(_keyboardCache);
        var charResponse2 = new KeyboardCharCache(_keyboardCache);
        _keyboardCache.Char('a');
        _keyboardCache.Char('b');
        _keyboardCache.Char('c');
        _charResponse.ReadCache().ShouldBe("abc");
        charResponse1.ReadCache().ShouldBe("abc");
        charResponse2.ReadCache().ShouldBe("abc");
    }

    [Test]
    public void ReadChars_Multiple_Handlers_Mixed_Reads()
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
        charResponse1Read1.ShouldBe("a");
        charResponse2Read1.ShouldBe("ab");
        charResponse3Read1.ShouldBe("abc");
        charResponse1Read2.ShouldBe("bc");
        charResponse2Read2.ShouldBe("c");
        charResponse3Read2.ShouldBeEmpty();
    }

    public void Dispose()
        => _charResponse.Dispose();
}