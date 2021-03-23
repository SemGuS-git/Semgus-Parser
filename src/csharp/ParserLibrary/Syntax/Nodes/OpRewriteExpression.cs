using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Production rewrite expression consisting of an operator followed by one or more other symbols.
    /// </summary>
    public class OpRewriteExpression : IProductionRewriteExpression {
        public ParserRuleContext ParserContext { get; }
        public Operator Op { get; }
        public IReadOnlyList<IProductionRewriteAtom> Operands { get; }

        public OpRewriteExpression(ParserRuleContext parserContext, Operator op, IReadOnlyList<IProductionRewriteAtom> operands) {
            ParserContext = parserContext;
            Op = op;
            Operands = operands;
        }

        public bool IsLeaf() => Operands.Any(e => e is NonterminalTermDeclaration);
        
        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}