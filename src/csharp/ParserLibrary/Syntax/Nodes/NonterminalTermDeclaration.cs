namespace Semgus.Syntax {
    /// <summary>
    /// Declares a variable of type "Term" as a particular nonterminal.
    /// </summary>
    public class NonterminalTermDeclaration : VariableDeclaration, IProductionRewriteAtom {
        public const string TYPE_NAME = "Term";

        public Nonterminal Nonterminal { get; }
        
        public NonterminalTermDeclaration(string name, SemgusType type, Nonterminal nonterminal, Context declarationContext) : base(name, type, declarationContext) {
            this.Nonterminal = nonterminal;
        }
        
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}