using System.Collections.Generic;
using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Invocation of an externally defined function, e.g. "and", "or", "=".
    /// /// </summary>
    public class LibraryFunctionCall : IFormula {
        public ParserRuleContext ParserContext { get; }
        public LibraryFunction LibraryFunction { get; }
        public IReadOnlyList<IFormula> Arguments { get; }

        public LibraryFunctionCall(ParserRuleContext parserContext, LibraryFunction libraryFunction, IReadOnlyList<IFormula> arguments) {
            ParserContext = parserContext;
            LibraryFunction = libraryFunction;
            Arguments = arguments;
        }
        
        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}