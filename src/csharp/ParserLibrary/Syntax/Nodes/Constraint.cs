using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    /// <summary>
    /// A behavioral constraint on the function to be synthesized, specified by a boolean formula to be asserted.
    /// </summary>
    public class Constraint : ISyntaxNode {
        public SemgusParserContext ParserContext { get; set; }
        public VariableClosure Closure { get; }
        public IFormula Formula { get; }

        public Constraint(VariableClosure closure, IFormula formula) {
            Closure = closure;
            Formula = formula;
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}