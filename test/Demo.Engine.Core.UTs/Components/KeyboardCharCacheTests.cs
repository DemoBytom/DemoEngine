using Demo.Engine.Core.Components;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Requests.Keyboard;
using FluentAssertions;
using Xunit;

namespace Demo.Engine.Core.UTs.Components
{
    public class KeyboardCharCacheTests
    {
        private readonly IKeyboardCache _keyboardCache;
        private readonly KeyboardCharResponse _charResponse;

        public KeyboardCharCacheTests()
        {
            _keyboardCache = new KeyboardCache();
            _charResponse = new KeyboardCharResponse(_keyboardCache);
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
    }
}