using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Components.Keyboard.Internal;
using Demo.Engine.Core.Interfaces.Components;
using FluentAssertions;
using Xunit;

namespace Demo.Engine.Core.UTs.Components.Keyboard
{
    public class KeyboardCharCacheTests
    {
        private readonly IKeyboardCache _keyboardCache;
        private readonly KeyboardCharCache _charResponse;

        public KeyboardCharCacheTests()
        {
            _keyboardCache = new KeyboardCache();
            _charResponse = new KeyboardCharCache(_keyboardCache);
        }

        [Fact]
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
            result.Should().Be("ala ma kota");
        }

        [Fact]
        public void ReadChars_Dequeues_Properly()
        {
            _keyboardCache.Char('a');
            _keyboardCache.Char('b');
            _keyboardCache.Char('c');

            var result1 = _charResponse.ReadCache();
            var result2 = _charResponse.ReadCache();

            result1.Should().Be("abc");
            result2.Should().BeEmpty();
        }

        [Fact]
        public void ReadChars_From_Multiple_Handlers()
        {
            var charResponse1 = new KeyboardCharCache(_keyboardCache);
            var charResponse2 = new KeyboardCharCache(_keyboardCache);

            _keyboardCache.Char('a');
            _keyboardCache.Char('b');
            _keyboardCache.Char('c');

            _charResponse.ReadCache().Should().Be("abc");
            charResponse1.ReadCache().Should().Be("abc");
            charResponse2.ReadCache().Should().Be("abc");
        }

        [Fact]
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

            charResponse1Read1.Should().Be("a");
            charResponse2Read1.Should().Be("ab");
            charResponse3Read1.Should().Be("abc");

            charResponse1Read2.Should().Be("bc");
            charResponse2Read2.Should().Be("c");
            charResponse3Read2.Should().BeEmpty();
        }
    }
}