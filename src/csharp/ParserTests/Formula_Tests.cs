using Antlr4.Runtime;
using Semgus.Parser.Internal;
using System;
using Xunit;
using static Semgus.Parser.Internal.SemgusParser;

namespace ParserTests
{
    public class Formula_Tests {
        [Theory]
        [InlineData("+",1,2)]
        [InlineData("+", 100, 0)]
        [InlineData("+", -5, -2)]
        public void ParseIntegerAddition(string op, int a, int b) {
            var codeText = $"({op} {a} {b})";

            var lexer = new SemgusLexer(new AntlrInputStream(codeText));
            var parser = new SemgusParser(new CommonTokenStream(lexer));

            var cst = parser.formula();

            var subformulas = cst.formula();
            Assert.Equal(3, subformulas.Length);

            var context0 = subformulas[0].symbol();
            Assert.NotNull(context0);
            Assert.Equal(op, context0.GetText());

            AssertIntLiteral(subformulas[1], a);
            AssertIntLiteral(subformulas[2], b);
        }

        void AssertIntLiteral(FormulaContext context, int value) {
            var lit = context.literal();
            Assert.NotNull(lit);
            var val = lit.intConst();
            Assert.NotNull(val);
            Assert.Equal(value, int.Parse(val.GetText()));
        }
    }
}
