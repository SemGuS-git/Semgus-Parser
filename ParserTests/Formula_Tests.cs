using System.Collections.Generic;
using System.Linq;

using Semgus.Parser.Reader;

using Xunit;

namespace ParserTests
{
    public class String_Tests {
        [Theory]
        [MemberData(nameof(Data))]
        public void ParseStringDQ(string payload) {
            var text = $"\"{payload}\"";

            var reader = new SemgusReader(text);
            var token = reader.Read();

            var strToken = Assert.IsType<StringToken>(token);

            Assert.Equal(payload, strToken.Value);
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
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        public void ParsePositiveIntegerAddition(string op, int a, int b) {/* TODO
            var codeText = $"({op} {Encode(a)} {Encode(b)})";

            var reader = new SemgusReader(codeText);
            var token = reader.Read();

            var consToken = Assert.IsType<ConsToken>(token);
            Assert.True(FormulaForm.TryParse(consToken, out var formula, out var _, out var _));

            Assert.NotNull(formula.List);
            Assert.Equal(3, formula.List.Count);

            var opAtom = formula.List[0].Atom;
            Assert.NotNull(opAtom);
            var opSym = Assert.IsType<SymbolToken>(opAtom);
            Assert.Equal(op, opSym.Name);

            AssertIntLiteral(formula.List[1], a);
            AssertIntLiteral(formula.List[2], b);*/
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        }

        static string Encode(int literal) => literal < 0 ? $"(- {-literal})" : $"{literal}";
        /* TODO
        void AssertIntLiteral(FormulaForm formula, int value) {
            Assert.NotNull(formula.Atom);
            var intLit = Assert.IsType<NumeralToken>(formula.Atom);
            Assert.Equal(value, intLit.Value);
    }*/
    }
}
