using System;
using System.Collections.Generic;
using System.IO;

using Semgus.Parser.Forms;
using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;
using Semgus.Syntax;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// Command for adding a new constraint into the SemGuS problem
    /// Syntax: (constraint [predicate])
    /// </summary>
    public class ConstraintCommand : ISemgusCommand
    {
        /// <summary>
        /// Name of this command
        /// </summary>
        public string CommandName => "constraint";

        /// <summary>
        /// Processes a constraint command
        /// </summary>
        /// <param name="previous">The SemgusProblem, prior to invocation of this command</param>
        /// <param name="form">The command form</param>
        /// <returns>The new SemgusProblem with the added constraint</returns>
        public SemgusProblem Process(SemgusProblem previous, ConsToken form, TextWriter errorStream, ref int errCount)
        {
            string err;
            SexprPosition errPos;

            // First element: the "constraint" symbol
            if (!form.TryPop(out SymbolToken _, out form, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
                return default;
            }

            // Second element: the predicate
            if (!form.TryPop(out SemgusToken formulaForm, out form, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
                return default;
            }

            if (!FormulaForm.TryParse(formulaForm, out FormulaForm formula, out err, out errPos))
            {
                errorStream.WriteParseError(err, errPos);
                errCount += 1;
                return default;
            }

            // Only two arguments permitted
            if (default != form)
            {
                errorStream.WriteParseError("Extra data at end of constraint command.", form.Position);
                errCount += 1;
                return default;
            }

            var closure = new VariableClosure(previous.GlobalClosure, new[] {
                // Temp: hardcode t:Term into constraint context as auxiliary variable
                new VariableDeclaration(name: "t",
                                        type: previous.GlobalEnvironment.ResolveType(NonterminalTermDeclaration.TYPE_NAME),
                                        declarationContext: VariableDeclaration.Context.CT_Term) // TODO { ParserContext = context}
            });

            var constraint = new Constraint(
                closure: closure,
                formula: new FormulaConverter(previous.GlobalEnvironment, closure).ConvertFormula(formula)
            );

            return previous.AddConstraint(constraint);
        }
    }
}
