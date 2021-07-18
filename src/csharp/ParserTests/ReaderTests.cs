using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Parser.Reader;

using Xunit;

namespace ParserTests
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

            Assert.True(cons.TryPop(out SymbolToken symb, out cons, out _, out _));
            Assert.Equal("list", symb.Name);

            Assert.True(cons.TryPop(out NumeralToken num, out cons, out _, out _));
            Assert.Equal(1, num.Value);

            Assert.True(cons.TryPop(out num, out cons, out _, out _));
            Assert.Equal(2, num.Value);

            Assert.True(cons.TryPop(out num, out cons, out _, out _));
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
    }
}
