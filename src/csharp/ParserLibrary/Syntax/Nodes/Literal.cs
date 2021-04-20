using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Semgus.Parser.Internal;

namespace Semgus.Syntax {
    /// <summary>
    /// Static convenience class.
    /// </summary>
    public static class Literal {
        public static LiteralBase Convert([NotNull] SemgusParser.LiteralContext context) => LiteralConverter.Default.Visit(context) ?? throw new NotSupportedException();
    }

    public class Literal<TValue> : LiteralBase {
        public override ParserRuleContext ParserContext { get; set; }

        public TValue Value { get; }
        public override Type ValueType => typeof(TValue);
        public override object BoxedValue => Value;

        public Literal(TValue value) {
            Value = value;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit<TValue>(this);

        public override string PrintFormula() => Value.ToString();
    }
}