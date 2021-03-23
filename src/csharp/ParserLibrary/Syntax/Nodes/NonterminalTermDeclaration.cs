using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Declares a variable of type "Term" as a particular nonterminal.
    /// </summary>
    public class NonterminalTermDeclaration : VariableDeclaration, IProductionRewriteAtom {
        public const string TYPE_NAME = "Term";

        public Nonterminal Nonterminal { get; }

        public NonterminalTermDeclaration(ParserRuleContext parserContext, string name, SemgusType type, Nonterminal nonterminal, SemanticUsage usage) : base(parserContext, name, type, usage) {
            this.Nonterminal = nonterminal;
        }
        
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}