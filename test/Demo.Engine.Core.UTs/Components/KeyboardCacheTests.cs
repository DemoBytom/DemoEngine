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
            var result = _keyboardCache.ReadChars();

            // Assert
            result.Should().Be("ala ma kota");
        }

        [Fact]
        public void ReadChars_Dequeues_Properly()
        {
            _keyboardCache.Char('a');
            _keyboardCache.Char('b');
            _keyboardCache.Char('c');

            var result1 = _keyboardCache.ReadChars();
            var result2 = _keyboardCache.ReadChars();

            result1.Should().Be("abc");
            result2.Should().BeEmpty();
        }
    }
}