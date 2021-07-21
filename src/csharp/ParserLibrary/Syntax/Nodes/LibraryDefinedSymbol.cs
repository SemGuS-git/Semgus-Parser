
using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    /// <summary>
    /// Retrieval of an externally defined constant whose text representation is a symbol, e.g. "true", "false"
    /// </summary>
    public class LibraryDefinedSymbol : IFormula {
        public SemgusParserContext ParserContext { get; set; }
        public string Identifier { get; }

        public LibraryDefinedSymbol(string identifier) {
            Identifier = identifier;
        }

        // these should never be called
        public T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
        public string PrintFormula() => Identifier;
    }
}