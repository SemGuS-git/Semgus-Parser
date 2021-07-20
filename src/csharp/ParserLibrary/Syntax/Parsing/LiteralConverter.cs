using System;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Semgus.Parser.Internal;

namespace Semgus.Syntax {
    /// <summary>
    /// Visitor that converts literal CST nodes into corresponding generic-typed literal AST nodes.
    /// TODO: throw exception when visiting non-literal contexts
    /// </summary>
    public class LiteralConverter : SemgusBaseVisitor<LiteralBase> {
        public static LiteralConverter Default { get; } = new LiteralConverter();

        private LiteralConverter() { }
        
        public override LiteralBase VisitIntConst([NotNull] SemgusParser.IntConstContext context) =>
            new Literal<int>( value: int.Parse(context.GetText())) {ParserContext = context};

        public override LiteralBase VisitBoolConst([NotNull] SemgusParser.BoolConstContext context) =>
            new Literal<bool>( value: bool.Parse(context.GetText())) {ParserContext = context};

        public override LiteralBase VisitBVConst([NotNull] SemgusParser.BVConstContext context) =>
            new Literal<SmtBitVec32>(value: SmtBitVec32.Parse(context.GetText())) { ParserContext = context };

        public override LiteralBase VisitEnumConst([NotNull] SemgusParser.EnumConstContext context) =>
            throw new NotImplementedException();
            
        public override LiteralBase VisitRealConst([NotNull] SemgusParser.RealConstContext context) =>
            new Literal<double>( value: double.Parse(context.GetText())) {ParserContext = context};


        public override LiteralBase VisitQuotedLit([NotNull] SemgusParser.QuotedLitContext context) {
            if (context.DOUBLEQUOTEDLIT() != null) {
                return new Literal<string>( value: Regex.Match(context.GetText(), "\"([^\"]*)\"").Groups[1].Value) {ParserContext = context};
            }
            if (context.SINGLEQUOTEDLIT() != null) {
                return new Literal<string>( value: Regex.Match(context.GetText(), "'([^']*)'").Groups[1].Value) {ParserContext = context};
            }

            throw new ArgumentException();
        }
    }

}