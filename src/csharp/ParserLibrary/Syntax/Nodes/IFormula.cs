namespace Semgus.Syntax {
    /// <summary>
    /// An arbitrary term in an SMT-LIB2 formula.
    /// </summary>
    public interface IFormula : ISyntaxNode {
        string PrintFormula();
    }
}