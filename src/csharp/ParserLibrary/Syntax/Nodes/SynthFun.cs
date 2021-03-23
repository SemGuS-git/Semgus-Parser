using System.Collections.Generic;
using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// The function under synthesis.
    /// Its list of productions acts as the specification for the DSL.
    /// </summary>
    public class SynthFun : ISyntaxNode {
        public ParserRuleContext ParserContext { get; }
        public string Name { get; }
        public VariableClosure Closure { get; }
        public IReadOnlyList<Production> Productions { get; }

        public SynthFun(ParserRuleContext parserContext, string name, VariableClosure closure, IReadOnlyList<Production> productions) {
            ParserContext = parserContext;
            Name = name;
            Closure = closure;
            Productions = productions;
        }
        
        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}