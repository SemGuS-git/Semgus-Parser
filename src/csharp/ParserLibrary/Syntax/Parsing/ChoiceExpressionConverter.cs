using System;
using System.Collections.Generic;
using System.Linq;

using Semgus.Parser.Forms;
using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    public class ChoiceExpressionConverter
    {
        private readonly LanguageEnvironment _env;
        private readonly Dictionary<string,NonterminalTermDeclaration> _declaredTerms;
        
        public IReadOnlyCollection<NonterminalTermDeclaration> DeclaredTerms => _declaredTerms.Values;

        public ChoiceExpressionConverter(LanguageEnvironment env)
        {
            _env = env;
            _declaredTerms = new Dictionary<string, NonterminalTermDeclaration>();
        }

        public IProductionRewriteExpression ProcessChoiceExpression(ProductionRuleForm rule)
        {
            if (rule.Operator is not null)
            {
                // TODO: name isn't necessarily a symbol...
                return new OpRewriteExpression(
                    op: new Operator(NameForToken(rule.Operator.Name)),
                    operands: rule.Operator.Parameters.Select(ProcessOperatorParameter).ToList()
                );
            }
            else if (rule.Leaf is not null)
            {
                if (rule.Leaf.IsLiteral)
                {
                    return new AtomicRewriteExpression(atom: Literal.Convert(rule.Leaf));
                }
                else if (rule.Leaf is SymbolToken symb)
                {
                    return new AtomicRewriteExpression(atom: new LeafTerm(NameForToken(rule.Leaf)));
                }
                else
                {
                    throw new InvalidOperationException("Not a supported leaf token type: " + rule.Leaf.ToString());
                }
            }
            else
            {
                throw new InvalidOperationException("Not an operator or leaf: " + rule.ToString());
            }
        }
        
        private string NameForToken(SemgusToken token)
        {
            if (token is SymbolToken symb)
            {
                return symb.Name;
            }
            else if (token is NumeralToken num)
            {
                return num.Value.ToString();
            }
            else if (token is DecimalToken dec)
            {
                return dec.Value.ToString();
            }
            else
            {
                throw new InvalidOperationException("Not a valid operator or leaf name: " + token.ToString());
            }
        }

        private IProductionRewriteAtom ProcessOperatorParameter(OperatorParameterForm opForm)
        {
            NonterminalTermDeclaration term = new(
                name: opForm.TermName.Name,
                type: _env.ResolveType(NonterminalTermDeclaration.TYPE_NAME),
                nonterminal: _env.ResolveNonterminal(opForm.Nonterminal.Name),
                declarationContext: VariableDeclaration.Context.PR_Subterm
            );

            _declaredTerms.Add(term.Name, term);

            return term;
        }
    }
}