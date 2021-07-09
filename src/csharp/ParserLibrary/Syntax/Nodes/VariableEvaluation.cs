using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    /// <summary>
    /// Use of a variable in a formula
    /// </summary>
    public class VariableEvaluation : IFormula {
        public SemgusParserContext ParserContext { get; set; }
        public VariableDeclaration Variable { get; }

        public VariableEvaluation(VariableDeclaration variable) {
            Variable = variable;
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);

        public string PrintFormula() => Variable.Name;
    }
}