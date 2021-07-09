using System;
using System.Collections.Generic;

using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Forms
{
    /// <summary>
    /// A form holding a production (a.k.a. nonterminal or CHC head)
    /// Syntax: ([name] [termVar] ([relationDefn]) (([varDecls]*) [relation]) [premises]*)
    /// </summary>
    public record ProductionForm(SymbolToken Name,
                                 SymbolToken Term,
                    SemanticRelationDefinitionForm RelationDefinition,
            IReadOnlyList<VariableDeclarationForm> VariableDeclarations,
                                       FormulaForm Relation,
                        IReadOnlyList<ProductionRuleForm> Premises)

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

            // This non-terminal's name
            if (!form.TryPop(out SymbolToken name, out form, out err, out errPos))
            {
                return false;
            }

            // The term variable in the relation
            if (!form.TryPop(out SymbolToken term, out form, out err, out errPos))
            {
                return false;
            }

            // The relation definition, e.g. (Start.Sem (Term Int Int Int))
            if (!form.TryPop(out ConsToken relDefnForm, out form, out err, out errPos))
            {
                return false;
            }
            if (!SemanticRelationDefinitionForm.TryParse(relDefnForm, out var relDefn, out err, out errPos))
            {
                return false;
            }

            // The CHC conclusion and variable declarations
            // This is in a separate sub-list, e.g.: (((x Int)) (X.Sem t x))
            if (!form.TryPop(out ConsToken varsAndConclusion, out form, out err, out errPos))
            {
                return false;
            }
            if (!varsAndConclusion.TryPop(out SemgusToken varDeclsForm, out varsAndConclusion, out err, out errPos))
            {
                return false;
            }
            if (!VariableDeclarationForm.TryParseList(varDeclsForm, out var varDecls, out err, out errPos))
            {
                return false;
            }
            if (!varsAndConclusion.TryPop(out ConsToken conclusionForm, out varsAndConclusion, out err, out errPos))
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

            production = new ProductionForm(name, term, relDefn, varDecls, conclusion, premises);
            return true;
        }
    }
}
