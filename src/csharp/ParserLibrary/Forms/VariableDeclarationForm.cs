using System;
using System.Collections.Generic;

using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Forms
{
    /// <summary>
    /// Form holding the declaration of a variable
    /// </summary>
    public record VariableDeclarationForm(SymbolToken Name, SymbolToken Type)
    {
        /// <summary>
        /// Tries to parse a variable declaration from the given cons
        /// </summary>
        /// <param name="cons">The form to parse</param>
        /// <param name="decl">The resultant declaration</param>
        /// <param name="err">Error message</param>
        /// <param name="errPos">Position of error</param>
        /// <returns>True if parsed successfully, false otherwise</returns>
        public static bool TryParse(ConsToken cons, out VariableDeclarationForm decl, out string err, out SexprPosition errPos)
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
                err = "Additional content at the end of variable declaration.";
                errPos = cons.Position;
                return false;
            }

            decl = new VariableDeclarationForm(name, type);
            err = default;
            errPos = default;
            return true;
        }

        /// <summary>
        /// Tries to parse a variable declaration list from the given form
        /// </summary>
        /// <param name="form">The form to parse (or Nil if empty)</param>
        /// <param name="decls">The resultant declarations</param>
        /// <param name="err">Error message</param>
        /// <param name="errPos">Position of error</param>
        /// <returns>True if parsed successfully, false otherwise</returns>
        public static bool TryParseList(SemgusToken form, out IReadOnlyList<VariableDeclarationForm> decls, out string err, out SexprPosition errPos)
        {
            List<VariableDeclarationForm> list = new();
            if (form is ConsToken cons)
            {
                do
                {
                    if (!cons.TryPop(out ConsToken declForm, out cons, out err, out errPos))
                    {
                        decls = default;
                        return false;
                    }

                    if (!TryParse(declForm, out VariableDeclarationForm decl, out err, out errPos))
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
                err = "Expected variable declaration list, but got: " + form.GetType().Name;
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
