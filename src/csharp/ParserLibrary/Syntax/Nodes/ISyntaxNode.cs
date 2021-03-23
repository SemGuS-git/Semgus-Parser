using Antlr4.Runtime;
using Semgus.Parser;

namespace Semgus.Syntax {
    public interface ISyntaxNode {
        // Original context in the CST from which this node was derived
        ParserRuleContext ParserContext { get; }
        
        // Visitor pattern hook
        T Accept<T>(IAstVisitor<T> visitor);
    }
}