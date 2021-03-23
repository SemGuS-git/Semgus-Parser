using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Production rewrite expression consisting of a single atomic element.
    /// </summary>
    public class AtomicRewriteExpression : IProductionRewriteExpression {
        public ParserRuleContext ParserContext { get; }
        public IProductionRewriteAtom Atom { get; }

        public AtomicRewriteExpression(ParserRuleContext parserContext, IProductionRewriteAtom atom) {
            ParserContext = parserContext;
            Atom = atom;
        }

        public bool IsLeaf() => true;
        
        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}