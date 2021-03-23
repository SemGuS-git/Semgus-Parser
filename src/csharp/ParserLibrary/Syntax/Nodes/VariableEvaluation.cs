using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Use of a variable in a formula
    /// </summary>
    public class VariableEvaluation : IFormula {
        public ParserRuleContext ParserContext { get; }
        public VariableDeclaration Variable { get; }

        public VariableEvaluation(ParserRuleContext parserContext, VariableDeclaration variable) {
            ParserContext = parserContext;
            Variable = variable;
        }
        
        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}