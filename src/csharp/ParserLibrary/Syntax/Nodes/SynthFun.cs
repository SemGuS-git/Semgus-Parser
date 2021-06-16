using System.Collections.Generic;
using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// The function under synthesis.
    /// Its list of productions acts as the specification for the DSL.
    /// </summary>
    public class SynthFun : ISyntaxNode {
        public ParserRuleContext ParserContext { get; set; }
        public string Name { get; }
        public VariableClosure Closure { get; }
        public IReadOnlyList<ProductionGroup> Productions { get; }

        public SynthFun( string name, VariableClosure closure, IReadOnlyList<ProductionGroup> productions) {
            Name = name;
            Closure = closure;
            Productions = productions;
        }
        
        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}