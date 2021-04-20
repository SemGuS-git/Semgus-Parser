using Antlr4.Runtime;

namespace Semgus.Syntax {
    public class VariableDeclaration : ISyntaxNode {
        public enum Context {
            SF_Input,
            SF_Output,
            NT_Term,
            NT_Auxiliary,
            PR_Subterm,
            PR_Auxiliary,
            CT_Term,
            CT_Auxiliary,
        };

        public ParserRuleContext ParserContext { get; set; }
        public string Name { get; }
        public SemgusType Type { get; }
        public Context DeclarationContext { get; }

        public VariableDeclaration(string name, SemgusType type, Context declarationContext) {
            Name = name;
            Type = type;
            DeclarationContext = declarationContext;
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}