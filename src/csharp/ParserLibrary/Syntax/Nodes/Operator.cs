using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Symbol that appears in a nonterminal rewrite expression with non-leaf arguments
    /// </summary>
    public class Operator : ISyntaxNode {
        public ParserRuleContext ParserContext { get; }
        public string Text { get; }

        public Operator(ParserRuleContext parserContext, string text) {
            ParserContext = parserContext;
            Text = text;
        }
        
        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}