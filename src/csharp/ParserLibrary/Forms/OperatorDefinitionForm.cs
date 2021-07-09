using System;
using System.Collections.Generic;

using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Forms
{
    /// <summary>
    /// A form holding the definition of an operator.
    /// E.g., (+ (Start t1) (Start t2))
    /// </summary>
    public record OperatorDefinitionForm(SemgusToken Name, IReadOnlyList<OperatorParameterForm> Parameters)
    {
        /// <summary>
        /// Tries to parse an operator definition
        /// </summary>
        /// <param name="form">The form to parse</param>
        /// <param name="opDefn">The resultant operator definition</param>
        /// <param name="err">Error message</param>
        /// <param name="errPos">Where the error occurred</param>
        /// <returns>True if parsed successfully, false if not</returns>
        public static bool TryParse(ConsToken form, out OperatorDefinitionForm opDefn, out string err, out SexprPosition errPos)
        {
            opDefn = default;

            if (!form.TryPop(out SemgusToken name, out form, out err, out errPos))
            {
                return false;
            }

            // The operator parameter definition list is the tail of form, not a sublist
            if (!OperatorParameterForm.TryParseList(form, out var paramDecls, out err, out errPos))
            {
                return false;
            }

            opDefn = new OperatorDefinitionForm(name, paramDecls);
            return true;
        }
    }
}
