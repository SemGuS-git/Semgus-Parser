using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Abstract base class for all literals, which should be generic subclasses.
    /// </summary>
    public abstract class LiteralBase : IFormula, IProductionRewriteAtom {
        public abstract ParserRuleContext ParserContext { get; }
        
        public abstract T Accept<T>(IAstVisitor<T> visitor);
    }
}