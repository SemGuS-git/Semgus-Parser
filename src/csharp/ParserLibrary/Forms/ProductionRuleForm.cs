using System;
using System.Collections.Generic;

using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Forms
{
    /// <summary>
    /// A form holding a production rule (a.k.a. RHS of a CHC). Note that it is either a leaf or an operator.
    /// Syntax (leaf)    : ([name] ([decls]*) [predicate])
    /// Syntax (operator): (([name] [opParams]*) ([decls]*) [predicate]) 
    /// </summary>
    public record ProductionRuleForm(SemgusToken Leaf, OperatorDefinitionForm Operator, IReadOnlyList<VariableDeclarationForm> VariableDeclarations, FormulaForm Predicate)
    {
        /// <summary>
        /// Tries to parse a production rule (a.k.a. RHS of a production) from the given form
        /// </summary>
        /// <param name="form">The form to parse</param>
        /// <param name="rule">The resultant rule</param>
        /// <param name="err">Error message</param>
        /// <param name="errPos">Where the error occurred</param>
        /// <returns>True if parsed successfully, false if not</returns>
        public static bool TryParse(ConsToken form, out ProductionRuleForm rule, out string err, out SexprPosition errPos)
        {
            rule = default;
            OperatorDefinitionForm opDefn = default;

            if (!form.TryPop(out SemgusToken name, out form, out err, out errPos))
            {
                return false;
            }

            if (name is ConsToken opCons)
            {
                if (!OperatorDefinitionForm.TryParse(opCons, out opDefn, out err, out errPos))
                {
                    return false;
                }
                name = default;
            }

            if (!form.TryPop(out SemgusToken varDeclForm, out form, out err, out errPos))
            {
                return false;
            }
            if (!VariableDeclarationForm.TryParseList(varDeclForm, out var varDecls, out err, out errPos))
            {
                return false;
            }

            if (!form.TryPop(out SemgusToken predicateForm, out form, out err, out errPos))
            {
                return false;
            }
            if (!FormulaForm.TryParse(predicateForm, out var predicate, out err, out errPos))
            {
                return false;
            }

            if (default != form)
            {
                err = "Extra data at end of production rule declaration.";
                errPos = form.Position;
                return false;
            }

            rule = new ProductionRuleForm(name, opDefn, varDecls, predicate);
            return true;
        }
    }
}
