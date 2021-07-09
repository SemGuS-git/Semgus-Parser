using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    /// <summary>
    /// Specifies a single semantic rule in the form of a predicate that, when true, permits some particular rewrite.
    /// </summary>
    public class SemanticRule : ISyntaxNode {
        public SemgusParserContext ParserContext { get; set; }
        public IProductionRewriteExpression RewriteExpression { get; }
        public VariableClosure Closure { get; }
        public IFormula Predicate { get; }

        public SemanticRule(IProductionRewriteExpression rewriteExpression, VariableClosure closure, IFormula predicate) {
            RewriteExpression = rewriteExpression;
            Closure = closure;
            Predicate = predicate;
        }

        // Indicates whether this rule is a leaf (zero nonterminal child terms) or a branch (one or more nonterminal child terms).
        public bool IsLeaf() => RewriteExpression.IsLeaf();

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}