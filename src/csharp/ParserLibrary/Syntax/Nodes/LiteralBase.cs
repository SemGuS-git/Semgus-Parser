using System;

using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    /// <summary>
    /// Abstract base class for all literals, which should be generic subclasses.
    /// </summary>
    public abstract class LiteralBase : IFormula, IProductionRewriteAtom {
        public abstract SemgusParserContext ParserContext { get; set; }
        
        public abstract T Accept<T>(IAstVisitor<T> visitor);
        
        public abstract Type ValueType {get;}
        public abstract object BoxedValue {get;}

        public abstract string PrintFormula();
    }
}