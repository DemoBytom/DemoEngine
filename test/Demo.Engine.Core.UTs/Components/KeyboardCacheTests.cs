using Demo.Engine.Core.Components;
using Demo.Engine.Core.Interfaces.Components;
using FluentAssertions;
using Xunit;

namespace Demo.Engine.Core.UTs.Components
{
    public class KeyboardCacheTests
    {
        private readonly IKeyboardCache _keyboardCache;

        public KeyboardCacheTests()
        {
            _keyboardCache = new KeyboardCache();
        }

        [Fact]
        public void ReadChars_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            _keyboardCache.OnChar('a');
            _keyboardCache.OnChar('l');
            _keyboardCache.OnChar('a');
            _keyboardCache.OnChar(' ');
            _keyboardCache.OnChar('m');
            _keyboardCache.OnChar('a');
            _keyboardCache.OnChar(' ');
            _keyboardCache.OnChar('k');
            _keyboardCache.OnChar('o');
            _keyboardCache.OnChar('t');
            _keyboardCache.OnChar('a');

            // Act
            var result = _keyboardCache.ReadChars();

            // Assert
            result.Should().Be("ala ma kota");
        }

        [Fact]
        public void ReadChars_Dequeues_Properly()
        {
            _keyboardCache.OnChar('a');
            _keyboardCache.OnChar('b');
            _keyboardCache.OnChar('c');

            var result1 = _keyboardCache.ReadChars();
            var result2 = _keyboardCache.ReadChars();

            result1.Should().Be("abc");
            result2.Should().BeEmpty();
        }
    }
}