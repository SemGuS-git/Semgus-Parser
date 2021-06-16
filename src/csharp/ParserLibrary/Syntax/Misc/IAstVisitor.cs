namespace Semgus.Syntax {
    public interface IAstVisitor<T> {
        T Visit(AtomicRewriteExpression node);
        T Visit(Constraint node);
        T Visit(LeafTerm node);
        T Visit(LibraryFunctionCall node);
        T Visit(NonterminalTermDeclaration node);
        T Visit(Operator node);
        T Visit(OpRewriteExpression node);
        T Visit(ProductionGroup node);
        T Visit(SemanticRule node);
        T Visit(SemanticRelationDeclaration node);
        T Visit(SemanticRelationInstance node);
        T Visit(SemanticRelationQuery node);
        T Visit(SemgusProblem node);
        T Visit(SynthFun node);
        T Visit(VariableDeclaration node);
        T Visit(VariableEvaluation node);
        T Visit<TValue>(Literal<TValue> node);
    }
}