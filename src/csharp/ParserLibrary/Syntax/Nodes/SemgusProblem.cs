using System.Collections.Generic;
using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Root node of the Semgus syntax tree.
    /// </summary>
    public class SemgusProblem : ISyntaxNode {
        public ParserRuleContext ParserContext { get; set; }
        public SynthFun SynthFun { get; }
        public IReadOnlyList<Constraint> Constraints { get; }

        public SemgusProblem( SynthFun synthFun, IReadOnlyList<Constraint> constraints) {
            this.SynthFun = synthFun;
            this.Constraints = constraints;
        }
        
        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}