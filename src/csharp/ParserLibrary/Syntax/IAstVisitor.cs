namespace Semgus.Parser {
    public interface IAstVisitor<T> {
        T Visit(EmptyFormulaNode node);
        T Visit(ProductionNode node);
        T Visit(FunctionApplicationNode node);
        T Visit(LiteralNodeBase node);
        T Visit(SymbolNode node);
        T Visit(SynthFunNode node);
        T Visit(VarDeclNode node);
        T Visit(StartNode node);
        T Visit(RhsNtAtomNode node);
        T Visit(RhsOpExpressionNode node);
        T Visit(PredicateNode node);
        T Visit(ProductionRhsNode node);
        T Visit(TypeNode node);
        T Visit(NtRelationNode node);
        T Visit(NtRelationDefNode node);
        T Visit(ProductionLhsNode node);
    }
}