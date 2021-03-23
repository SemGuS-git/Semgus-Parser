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
        public override ParserRuleContext ParserContext { get; }

        public TValue Value { get; }

        public Literal(ParserRuleContext parserContext, TValue value) {
            ParserContext = parserContext;
            Value = value;
        }
        
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit<TValue>(this);
    }
}