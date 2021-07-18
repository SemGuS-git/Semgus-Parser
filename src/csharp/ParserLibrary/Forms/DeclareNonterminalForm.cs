using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Forms
{
    /// <summary>
    /// A forward declaration of a nonterminal and its semantic relation
    /// </summary>
    public record DeclareNonterminalForm(SymbolToken Name, SymbolToken Type, SemanticRelationDefinitionForm RelationDefinition)
    {
        /// <summary>
        /// Tries to parse a nonterminal declaration
        /// </summary>
        /// <param name="cons">Form to parse. Should not include a 'declare-nt' or similar.</param>
        /// <param name="form">The declaration form</param>
        /// <param name="err">Error message</param>
        /// <param name="errPos">Position of error</param>
        /// <returns>True if parsed, false if error encountered</returns>
        public static bool TryParse(ConsToken cons, out DeclareNonterminalForm form, out string err, out SexprPosition errPos)
        {
            if (!cons.TryPop(out SymbolToken name, out cons, out err, out errPos))
            {
                form = default;
                return false;
            }

            if (!cons.TryPop(out SymbolToken type, out cons, out err, out errPos))
            {
                form = default;
                return false;
            }

            if (!cons.TryPop(out ConsToken relDecl, out cons, out err, out errPos))
            {
                form = default;
                return false;
            }

            if (cons is not null)
            {
                err = "Extra data after semantic relation definition: " + cons;
                errPos = cons.Position;
                form = default;
                return false;
            }

            if (!SemanticRelationDefinitionForm.TryParse(relDecl, out var relDeclForm, out err, out errPos))
            {
                form = default;
                return false;
            }

            form = new DeclareNonterminalForm(name, type, relDeclForm);
            return true;
        }
    }
}
