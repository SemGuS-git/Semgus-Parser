using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    public interface ISyntaxNode {
        // Original context in the S-expression forms from which this node was derived
        SemgusParserContext ParserContext { get; }
        
        // Visitor pattern hook
        T Accept<T>(IAstVisitor<T> visitor);
    }
}