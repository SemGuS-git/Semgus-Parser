using System;
using System.Collections.Generic;

using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Forms
{
    /// <summary>
    /// A form holding some (syntactically-)arbitrary SMT-LIB2 formula, with possible annotations
    /// </summary>
    public record FormulaForm(SemgusToken Atom, IReadOnlyList<FormulaForm> List, Annotation Annotation)
    {
        public SexprPosition Position
        { 
            get
            {
                if (Atom is not null)
                {
                    return Atom.Position;
                }
                else
                {
                    return List[0].Position;
                }
            }
        }

        /// <summary>
        /// Try to parse a formula from the given form
        /// </summary>
        /// <param name="form">The form to parse</param>
        /// <param name="formula">The resultant formula</param>
        /// <param name="err">Error message</param>
        /// <param name="errPos">Where the error occurred</param>
        /// <returns>True if parsed successfully, false if not</returns>
        public static bool TryParse(SemgusToken form, out FormulaForm formula, out string err, out SexprPosition errPos)
        {
            formula = default;
            Annotation annotation = default;

            if (Annotation.HasAnnotation(form))
            {
                if (!Annotation.TryParse(form, out form, out annotation, out err, out errPos))
                {
                    return false;
                }
            }

            if (form is ConsToken cons)
            {
                // Listify
                SexprPosition pos = cons.Position;
                List<FormulaForm> list = new();
                do
                {
                    if (!cons.TryPop(out SemgusToken head, out cons, out err, out errPos))
                    {
                        return false;
                    }
                    if (!TryParse(head, out FormulaForm subFormula, out err, out errPos))
                    {
                        return false;
                    }
                    list.Add(subFormula);
                }
                while (default != cons);
                formula = new FormulaForm(default, list, annotation);
                err = default;
                errPos = default;
                return true;
            }
            else
            {
                formula = new FormulaForm(form, default, annotation);
                err = default;
                errPos = default;
                return true;
            }
        }
    }
}
