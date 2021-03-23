using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Terminal symbol that appears in a production rewrite expression.
    /// </summary>
    public class LeafTerm : IProductionRewriteAtom {
        public ParserRuleContext ParserContext { get; }
        public string Text { get; }

        public LeafTerm(ParserRuleContext parserContext, string text) {
            ParserContext = parserContext;
            Text = text;
        }
        
        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}