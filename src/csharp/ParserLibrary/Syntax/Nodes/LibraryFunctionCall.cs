using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Invocation of an externally defined function, e.g. "and", "or", "=".
    /// /// </summary>
    public class LibraryFunctionCall : IFormula {
        public ParserRuleContext ParserContext { get; set; }
        public LibraryFunction LibraryFunction { get; }
        public IReadOnlyList<IFormula> Arguments { get; }

        public LibraryFunctionCall(LibraryFunction libraryFunction, IReadOnlyList<IFormula> arguments) {
            LibraryFunction = libraryFunction;
            Arguments = arguments;
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);

        public string PrintFormula() => $"({LibraryFunction.Name} {string.Join(" ", Arguments.Select(t => t.PrintFormula()))})";
    }
}