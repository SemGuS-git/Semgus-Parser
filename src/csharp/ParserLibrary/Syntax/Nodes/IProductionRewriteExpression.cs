namespace Semgus.Syntax {
    /// <summary>
    /// String of one or more symbols that specifies the rewrite associated with a particular production rule.
    /// aka "RHS expression"
    /// </summary>
    public interface IProductionRewriteExpression : ISyntaxNode {
        // Returns false iff this expresion declares one or more nonterminal term variables.
        bool IsLeaf();
    }
}