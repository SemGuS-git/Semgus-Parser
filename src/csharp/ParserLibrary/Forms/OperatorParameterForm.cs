using System;
using System.Collections.Generic;

using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Forms
{
    /// <summary>
    /// A form holding the parameter declaration of an operator.
    /// E.g., (Start t1)
    /// </summary>
    public record OperatorParameterForm(SymbolToken Nonterminal, SymbolToken TermName)
    {
        /// <summary>
        /// Tries to parse an operator parameter definition from the given form
        /// </summary>
        /// <param name="cons">The form to parse</param>
        /// <param name="decl">The resultant operator parameter declaration</param>
        /// <param name="err">Error message</param>
        /// <param name="errPos">Where the error occurred</param>
        /// <returns>True if parsed successfully, false if not</returns>
        public static bool TryParse(ConsToken cons, out OperatorParameterForm decl, out string err, out SexprPosition errPos)
        {
            // The variable name
            if (!cons.TryPop(out SymbolToken name, out cons, out err, out errPos))
            {
                decl = default;
                return false;
            }

            // The variable type
            if (!cons.TryPop(out SymbolToken type, out cons, out err, out errPos))
            {
                decl = default;
                return false;
            }

            // There shouldn't be anything extra!
            if (default != cons)
            {
                decl = default;
                err = "Additional content at the end of operator parameter declaration.";
                errPos = cons.Position;
                return false;
            }

            decl = new OperatorParameterForm(name, type);
            err = default;
            errPos = default;
            return true;
        }

        /// <summary>
        /// Tries to parse a list of operator parameter declarations
        /// </summary>
        /// <param name="form">The form to parse</param>
        /// <param name="decls">The resultant declaration list</param>
        /// <param name="err">Error message</param>
        /// <param name="errPos">Position of error</param>
        /// <returns>True if successfully parsed, false if not</returns>
        public static bool TryParseList(SemgusToken form, out IReadOnlyList<OperatorParameterForm> decls, out string err, out SexprPosition errPos)
        {
            List<OperatorParameterForm> list = new();
            if (form is ConsToken cons)
            {
                do
                {
                    if (!cons.TryPop(out ConsToken declForm, out cons, out err, out errPos))
                    {
                        decls = default;
                        return false;
                    }

                    if (!TryParse(declForm, out OperatorParameterForm decl, out err, out errPos))
                    {
                        decls = default;
                        return false;
                    }
                    list.Add(decl);
                }
                while (default != cons);

            }
            else if (form is not NilToken)
            {
                err = "Expected operator parameter list, but got: " + form.GetType().Name;
                errPos = form.Position;
                decls = default;
                return false;
            }

            decls = list;
            err = default;
            errPos = default;
            return true;
        }
    }
}
