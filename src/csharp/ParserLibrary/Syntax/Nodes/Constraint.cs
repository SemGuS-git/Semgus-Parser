using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// A behavioral constraint on the function to be synthesized, specified by a boolean formula to be asserted.
    /// </summary>
    public class Constraint : ISyntaxNode {
        public ParserRuleContext ParserContext { get; set; }
        public VariableClosure Closure { get; }
        public IFormula Formula { get; }

        public Constraint(VariableClosure closure, IFormula formula) {
            Closure = closure;
            Formula = formula;
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}