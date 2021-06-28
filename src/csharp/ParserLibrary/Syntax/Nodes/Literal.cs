using System;
using Antlr4.Runtime;
using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    /// <summary>
    /// Static convenience class.
    /// </summary>
    public static class Literal
    {
        /// <summary>
        /// Converts a Semgus token into an appropriate literal
        /// </summary>
        /// <param name="token">The token to convert</param>
        /// <returns>The converted literal</returns>
        /// <exception cref="InvalidOperationException">Thrown if the token does not represent a literal</exception>
        public static LiteralBase Convert(SemgusToken token) => token.Accept(LiteralConverter.Default);
    }

    public class Literal<TValue> : LiteralBase
    {
        public override ParserRuleContext ParserContext { get; set; }

        public TValue Value { get; }
        public override Type ValueType => typeof(TValue);
        public override object BoxedValue => Value;

        public Literal(TValue value)
        {
            Value = value;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);

        public override string PrintFormula() => Value.ToString();
    }
}