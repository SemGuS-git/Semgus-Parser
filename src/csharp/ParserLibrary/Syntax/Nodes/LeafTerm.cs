using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    /// <summary>
    /// Terminal symbol that appears in a production rewrite expression.
    /// </summary>
    public class LeafTerm : IProductionRewriteAtom {
        public SemgusParserContext ParserContext { get; set; }
        public string Text { get; }

        public LeafTerm(string text) {
            Text = text;
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}