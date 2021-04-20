using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using Semgus.Parser.Internal;

namespace Semgus.Syntax {
    public class ChoiceExpressionConverter {
        private readonly LanguageEnvironment _env;
        private readonly Dictionary<string,NonterminalTermDeclaration> _declaredTerms;
        
        public IReadOnlyCollection<NonterminalTermDeclaration> DeclaredTerms => _declaredTerms.Values;

        public ChoiceExpressionConverter(LanguageEnvironment env) {
            this._env = env;
            this._declaredTerms = new Dictionary<string, NonterminalTermDeclaration>();
        }

        public IProductionRewriteExpression ProcessChoiceExpression([NotNull] SemgusParser.Rhs_expressionContext context) {
            var atoms = context.rhs_atom();
            
            if(context.op() is SemgusParser.OpContext opContext) {
                return new OpRewriteExpression(
                    op: ProcessOperator(opContext),
                    operands: atoms.Select(ProcessAtom).ToList()
                ) {ParserContext = context};
            } else {
                if(atoms.Length != 1) throw new ArgumentException();
                
                return new AtomicRewriteExpression (
                    atom: ProcessAtom(atoms[0])
                ) {ParserContext = context};
            }
        }
        
        private Operator ProcessOperator([NotNull] SemgusParser.OpContext context) {
            return new Operator(
                text: context.symbol().GetText()
            ) {ParserContext = context};
        }

        private IProductionRewriteAtom ProcessAtom([NotNull] SemgusParser.Rhs_atomContext context) {
            if(context.nt_name() is SemgusParser.Nt_nameContext nt_nameContext) {
                var term = new NonterminalTermDeclaration(
                    name: context.nt_term().symbol().GetText(),
                    type: _env.ResolveType(NonterminalTermDeclaration.TYPE_NAME),
                    nonterminal: _env.ResolveNonterminal(nt_nameContext.symbol()),
                    declarationContext: VariableDeclaration.Context.PR_Subterm
                ) {ParserContext = context};
                
                _declaredTerms.Add(term.Name,term);
                
                return term;
            }
            
            if(context.literal() is SemgusParser.LiteralContext literalContext) {
                return Literal.Convert(literalContext);
            }
            
            if(context.symbol() is SemgusParser.SymbolContext symbolContext) {
                return new LeafTerm(
                    text: symbolContext.GetText()
                ) {ParserContext = context};
            }
            
            throw new NotSupportedException();
        }
    }
}