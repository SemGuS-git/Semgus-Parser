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
    /// A variable declaration form, supporting both the standard single variable 
    /// and multi-variable forms. (... (var1 var2 ...) type) or (... var type)
    /// </summary>
    public record DeclareVarForm(IReadOnlyList<SymbolToken> Symbols, SymbolToken Type)
    {
        /// <summary>
        /// Tries to parse a declare-var form from the given cons. This should start with the variables
        /// to declare, not any `declare-var` symbol or related.
        /// </summary>
        /// <param name="cons">The form to parse</param>
        /// <param name="form">The resultant declaration form</param>
        /// <param name="err">Error message</param>
        /// <param name="errPos">Position of error</param>
        /// <returns>True if parsed successfully, false otherwise</returns>
        public static bool TryParse(ConsToken cons, out DeclareVarForm form, out string err, out SexprPosition errPos)
        {
            if (!cons.TryPop(out SemgusToken symbols, out cons, out err, out errPos))
            {
                form = default;
                return false;
            }

            if (!cons.TryPop(out SymbolToken type, out cons, out err, out errPos))
            {
                form = default;
                return false;
            }

            //
            // There shouldn't be anything extra!
            //
            if (default != cons)
            {
                form = default;
                err = "Additional content at the end of variable declaration.";
                errPos = cons.Position;
                return false;
            }

            //
            // The symbols are either a cons of symbols or a single one
            //
            var symbolList = new List<SymbolToken>();
            if (symbols is SymbolToken symbol)
            {
                symbolList.Add(symbol);
            }
            else if (symbols is ConsToken symbolCons)
            {
                while (symbolCons is not null)
                {
                    if (!symbolCons.TryPop(out symbol, out symbolCons, out err, out errPos))
                    {
                        form = default;
                        return false;
                    }
                    symbolList.Add(symbol);
                }
            }
            else
            {
                form = default;
                err = "Expected symbol or list of symbols, but got: " + symbols;
                errPos = symbols.Position;
                return false;
            }

            //
            // Assemble the form
            //
            form = new DeclareVarForm(symbolList, type);
            return true;
        }
    }
}
