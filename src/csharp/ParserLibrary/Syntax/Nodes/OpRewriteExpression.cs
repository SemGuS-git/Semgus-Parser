using System.Collections.Generic;
using System.Linq;

using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    /// <summary>
    /// Production rewrite expression consisting of an operator followed by one or more other symbols.
    /// </summary>
    public class OpRewriteExpression : IProductionRewriteExpression {
        public SemgusParserContext ParserContext { get; set; }
        public Operator Op { get; }
        public IReadOnlyList<IProductionRewriteAtom> Operands { get; }

        public OpRewriteExpression(Operator op, IReadOnlyList<IProductionRewriteAtom> operands) {
            Op = op;
            Operands = operands;
        }

        public bool IsLeaf() => !Operands.Any(e => e is NonterminalTermDeclaration);
        
        public IEnumerable<NonterminalTermDeclaration> GetChildTerms() {
            foreach(var atom in Operands) {
                if(atom is NonterminalTermDeclaration dec) yield return dec;
            }
        }
        
        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
        
        public string GetNamingSymbol() => Op.Text;
    }
}