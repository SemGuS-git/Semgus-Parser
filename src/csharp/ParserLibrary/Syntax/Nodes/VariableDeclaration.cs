using Antlr4.Runtime;

namespace Semgus.Syntax {
    public class VariableDeclaration : ISyntaxNode {
        public enum SemanticUsage {
            Input,
            Output,
            Auxiliary,
        };

        public ParserRuleContext ParserContext { get; }
        public string Name { get; }
        public SemgusType Type { get; }
        public SemanticUsage Usage { get; }

        public VariableDeclaration(ParserRuleContext parserContext, string name, SemgusType type, SemanticUsage usage) {
            ParserContext = parserContext;
            Name = name;
            Type = type;
            Usage = usage;
        }
        
        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}