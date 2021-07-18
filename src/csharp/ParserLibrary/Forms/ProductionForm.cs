using System;
using System.Collections.Generic;

using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Forms
{
    /// <summary>
    /// A form holding a production (a.k.a. nonterminal or CHC head)
    /// Syntax: (([name] [termVar]) [relation] [productions]*)
    /// </summary>
    public record ProductionForm(SymbolToken Name,
                                 SymbolToken Term,
                                 FormulaForm Relation,
           IReadOnlyList<ProductionRuleForm> Productions)

    {
        /// <summary>
        /// Tries to parse a production from the given cons.
        /// </summary>
        /// <param name="form">The form to parse</param>
        /// <param name="production">The resultant production</param>
        /// <param name="err">Error message</param>
        /// <param name="errPos">Position of error</param>
        /// <returns>True if parsed successfully, false otherwise</returns>
        public static bool TryParse(ConsToken form, out ProductionForm production, out string err, out SexprPosition errPos)
        {
            production = default;

            //
            // First form is a list (name termvar)
            //
            if (!form.TryPop(out ConsToken nameAndTermVar, out form, out err, out errPos))
            {
                return false;
            }

            // This non-terminal's name
            if (!nameAndTermVar.TryPop(out SymbolToken name, out nameAndTermVar, out err, out errPos))
            {
                return false;
            }

            // The term variable in the relation
            if (!nameAndTermVar.TryPop(out SymbolToken term, out nameAndTermVar, out err, out errPos))
            {
                return false;
            }

            if (default != nameAndTermVar)
            {
                err = "Extra data at end of non-terminal definition: " + nameAndTermVar;
                errPos = nameAndTermVar.Position;
                return false;
            }

            // The CHC conclusion
            if (!form.TryPop(out ConsToken conclusionForm, out form, out err, out errPos))
            {
                return false;
            }
            if (!FormulaForm.TryParse(conclusionForm, out var conclusion, out err, out errPos))
            {
                return false;
            }

            // Grab all the RHS premises
            var premises = new List<ProductionRuleForm>();
            while (default != form)
            {
                if (!form.TryPop(out ConsToken premiseForm, out form, out err, out errPos))
                {
                    return false;
                }
                if (!ProductionRuleForm.TryParse(premiseForm, out var premise, out err, out errPos))
                {
                    return false;
                }
                premises.Add(premise);
            }

            production = new ProductionForm(name, term, conclusion, premises);
            return true;
        }
    }
}
