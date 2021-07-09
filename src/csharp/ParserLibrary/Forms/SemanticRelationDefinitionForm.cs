using System;
using System.Collections.Generic;

using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Forms
{
    /// <summary>
    /// Form holding a semantic relation definition, e.g. (Start.Sem (Term Int Int Int))
    /// Syntax: ([defnName] ([paramTypes]*))
    /// </summary>
    public record SemanticRelationDefinitionForm(SymbolToken Name, IReadOnlyList<SymbolToken> Types)
    {
        /// <summary>
        /// Tries to parse a semantic relation definition from the given form
        /// </summary>
        /// <param name="form">The form to parse</param>
        /// <param name="relDefn">The resultant definition</param>
        /// <param name="err">Error message</param>
        /// <param name="errPos">Position of error</param>
        /// <returns>True if parsed successfully, false otherwise</returns>
        public static bool TryParse(ConsToken form, out SemanticRelationDefinitionForm relDefn, out string err, out SexprPosition errPos)
        {
            relDefn = default;

            if (!form.TryPop(out SymbolToken name, out form, out err, out errPos))
            {
                return false;
            }

            if (!form.TryPop(out ConsToken typeList, out form, out err, out errPos))
            {
                return false;
            }

            var types = new List<SymbolToken>();
            while (default != typeList)
            {
                if (!typeList.TryPop(out SymbolToken type, out typeList, out err, out errPos))
                {
                    return false;
                }
                types.Add(type);
            }

            if (default != form)
            {
                err = "Extra data at tail of semantic relation definition.";
                errPos = form.Position;
                return false;
            }

            relDefn = new SemanticRelationDefinitionForm(name, types);
            return true;
        }
    }
}
