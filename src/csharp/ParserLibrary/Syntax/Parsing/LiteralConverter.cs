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
            new Literal<int>(parserContext: context, value: int.Parse(context.GetText()));

        public override LiteralBase VisitBoolConst([NotNull] SemgusParser.BoolConstContext context) =>
            new Literal<bool>(parserContext: context, value: bool.Parse(context.GetText()));

        public override LiteralBase VisitBVConst([NotNull] SemgusParser.BVConstContext context) =>
            throw new NotImplementedException();

        public override LiteralBase VisitEnumConst([NotNull] SemgusParser.EnumConstContext context) =>
            throw new NotImplementedException();
            
        public override LiteralBase VisitRealConst([NotNull] SemgusParser.RealConstContext context) =>
            new Literal<double>(parserContext: context, value: double.Parse(context.GetText()));


        public override LiteralBase VisitQuotedLit([NotNull] SemgusParser.QuotedLitContext context) {
            if (context.DOUBLEQUOTEDLIT() != null) {
                return new Literal<string>(parserContext: context, value: Regex.Match(context.GetText(), "\"([^\"]*)\"").Captures[0].Value);
            }
            if (context.SINGLEQUOTEDLIT() != null) {
                return new Literal<string>(parserContext: context, value: Regex.Match(context.GetText(), "'([^']*)'").Captures[0].Value);
            }

            throw new ArgumentException();
        }
    }

}