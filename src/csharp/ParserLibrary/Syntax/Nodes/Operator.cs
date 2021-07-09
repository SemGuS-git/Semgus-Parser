using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    /// <summary>
    /// Symbol that appears in a nonterminal rewrite expression with non-leaf arguments
    /// </summary>
    public class Operator : ISyntaxNode {
        public SemgusParserContext ParserContext { get; set; }
        public string Text { get; }

        public Operator( string text) {
            Text = text;
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}