using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Parser.Reader;

using Xunit;

using static Semgus.Parser.Tests.NullableAssertions;

namespace Semgus.Parser.Tests
{
    public class ReaderTests
    {
        /// <summary>
        /// https://github.com/SemGuS-git/Semgus-Parser/issues/23
        /// </summary>
        [Fact]
        public void HandlesCommentAtEndOfList()
        {
            SemgusReader reader = new(@"
(list 1 2 3
  ; This is a comment
)
");
            SemgusToken tok = reader.Read();
            var cons = Assert.IsType<ConsToken>(tok);
            AssertNotNull(cons);

            AssertTrue(cons.TryPop(out SymbolToken? symb, out cons, out _, out _));
            Assert.Equal("list", symb.Name);
            AssertNotNull(cons);

            AssertTrue(cons.TryPop(out NumeralToken? num, out cons, out _, out _));
            Assert.Equal(1, num.Value);
            AssertNotNull(cons);

            AssertTrue(cons.TryPop(out num, out cons, out _, out _));
            Assert.Equal(2, num.Value);
            AssertNotNull(cons);

            AssertTrue(cons.TryPop(out num, out cons, out _, out _));
            Assert.Equal(3, num.Value);

            Assert.Null(cons);
        }

        [Fact]
        public void ThrowsOnExtraRightParenthesis()
        {
            SemgusReader reader = new(@"(1)) (2)");
            reader.Read(); // (1);
            Assert.Throws<Exception>(() => reader.Read());
        }

        [Fact]
        public void ThrowsOnUnmatchedLeftParenthesis()
        {
            SemgusReader reader = new(@"((1)");
            Assert.Throws<Exception>(() => reader.Read());
        }

        [Theory] // https://github.com/SemGuS-git/Semgus-Parser/issues/19
        [InlineData("test", true, false)]
        [InlineData("1test", false, false)]
        [InlineData("0test", false, false)]
        [InlineData("0.test", false, false)]
        [InlineData("@test", true, true)]
        [InlineData(".test", true, true)]
        [InlineData("9.5.6", false, false)]
        public void ReadsSymbol(string symbolName, bool isCompliant, bool isInternal)
        {
            SemgusReader reader = new(symbolName);
            var symbol = Assert.IsType<SymbolToken>(reader.Read());
            Assert.Equal(symbolName, symbol.Name);
            Assert.Equal(isCompliant, symbol.IsSmtLibCompliant);
            Assert.Equal(isInternal, symbol.IsSmtLibInternal);
        }

        [Theory]
        [InlineData("01")]
        [InlineData("02.5")]
        public void ThrowsOnNumberWithLeadingZero(string token)
        {
            SemgusReader reader = new(token);
            Assert.ThrowsAny<Exception>(() => reader.Read());
        }

        [Theory]
        [InlineData("1.05", 1.05d)]
        [InlineData("0.0009", 0.0009)]
        public void ReadsDecimals(string token, double value)
        {
            SemgusReader reader = new(token);
            var @decimal = Assert.IsType<DecimalToken>(reader.Read());
            Assert.Equal(value, @decimal.Value);
        }
    }
}
