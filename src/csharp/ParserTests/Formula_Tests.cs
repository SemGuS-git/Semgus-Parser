using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Semgus.Parser.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Semgus.Parser.Internal.SemgusParser;

namespace ParserTests
{
    public class String_Tests {
        [Theory]
        [MemberData(nameof(Data))]
        public void ParseStringDQ(string payload) {
            var text = $"\"{payload}\"";

            var lexer = new SemgusLexer(new AntlrInputStream(text));
            var parser = new SemgusParser(new CommonTokenStream(lexer));
            var cst = parser.literal();

            QuotedLitContext qlContext;// = null;
            ITerminalNode terminal;// = qlContext.DOUBLEQUOTEDLIT();
            Assert.NotNull(qlContext = cst.quotedLit());
            Assert.NotNull(terminal = qlContext.DOUBLEQUOTEDLIT());
            
            Assert.Equal(text, terminal.Symbol.Text);
        }


        public static IEnumerable<object[]> Data => new[] {
            "",
            " ",
            "_",
            "-",
            "abc",
            "ab c",
            "def",
            "123",
            "!@#$%^&*()-=_+[]{};:/?\\|",
        }.Select(s => new[] { s });
    }
    public class Formula_Tests {
        [Theory]
        [InlineData("+",1,2)]
        [InlineData("+", 100, 0)]
        [InlineData("+", 5, 2)]
        public void ParsePositiveIntegerAddition(string op, int a, int b) {
            var codeText = $"({op} {Encode(a)} {Encode(b)})";

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

        static string Encode(int literal) => literal < 0 ? $"(- {-literal})" : $"{literal}";

        void AssertIntLiteral(FormulaContext context, int value) {
            var lit = context.literal();
            Assert.NotNull(lit);
            var val = lit.intConst();
            Assert.NotNull(val);
            Assert.Equal(value, int.Parse(val.GetText()));
        }
    }
}
