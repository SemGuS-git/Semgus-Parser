using System;
using System.Collections.Generic;
using System.Linq;

using Semgus.Parser.Forms;
using Semgus.Parser.Reader;
using Semgus.Parser.Util;

namespace Semgus.Syntax {
    /// <summary>
    /// Converts from a CST formula to an AST formula.
    /// 
    /// For now, we say that all formulas are defined by:
    ///   Formula    := VAR_NAME | LITERAL | (Invocation Formula+)
    ///   Invocation := LIBRARY_FUNC_NAME | RELATION_NAME
    /// </summary>
    public class FormulaConverter {

        /// <summary>
        /// Temporary node to represent the appearance of some symbol.
        /// If it is the first symbol in its group, it will be resolved to a semantic relation or library function;
        /// otherwise, it must resolve to a variable name.
        /// </summary>
        private class SymbolicPlaceholder : IFormula {
            public SemgusParserContext ParserContext { get; set; }
            public string Identifier { get; }

            public SymbolicPlaceholder(string identifier) {

                Identifier = identifier;
            }

            // these should never be called
            public T Accept<T>(IAstVisitor<T> visitor) => throw new InvalidOperationException();
            public string PrintFormula() => throw new InvalidOperationException();
        }

        private readonly LanguageEnvironment _env;
        private readonly VariableClosure _closure;

        public FormulaConverter(LanguageEnvironment env, VariableClosure closure) {
            _env = env;
            _closure = closure;
        }

        public IFormula ConvertFormula(FormulaForm form) {
            var result = ConvertFormulaInner(form);
            if (result is SymbolicPlaceholder) {
                throw new Exception(); // probably an unbound variable
            }
            return result;
        }

        private IFormula ConvertFormulaInner(FormulaForm form)
        {

            if (form.Atom is not null)
            {
                if (form.Atom is SymbolToken symbol)
                {
                    var id = symbol.Name;
                    if (_closure.TryResolve(id, out var variable))
                    {
                        return new VariableEvaluation(variable); // TODO { ParserContext = context };
                    }
                    else
                    {
                        return new SymbolicPlaceholder(id); // TODO { ParserContext = context };
                    }
                }
                else if (form.Atom.IsLiteral)
                {
                    return Literal.Convert(form.Atom);
                }
                else
                {
                    throw new InvalidOperationException("Non-literal, non-symbol atom encountered in formula context: " + form.Atom);
                }
            }
            else if (form.List is not null)
            {
                if (form.List.Count == 0)
                {
                    // This shouldn't happen
                    throw new InvalidOperationException("Empty formula encountered");
                }

                // Application
                var subformulae = form.List.Select(ConvertFormulaInner);
                var arguments = subformulae.Pop(out var head);
                if (head is SymbolicPlaceholder placeholder)
                {
                    return MakeInvocation(form.Position, placeholder.Identifier, arguments.ToList());
                }
                else
                {
                    throw new InvalidOperationException("Expected symbol at head of formula, but got: " + head);
                }
            }

            // TODO throw new SemgusSyntaxException(context, "Invalid formula");
            throw new InvalidOperationException("Invalid formula");
        }

        private IFormula MakeInvocation(SemgusParserContext context, string identifier, List<IFormula> args) {
            foreach (var arg in args) {
                if (arg is SymbolicPlaceholder ph)
                {
                    // TODO throw new SemgusSyntaxException(ph.ParserContext, $"Unknown identifier \"{ph.Identifier}\" (not a variable in [{_closure.PrintAllResolvableVariables()}])");
                    throw new InvalidOperationException($"Unknown identifier \"{ph.Identifier}\" (not a variable in [{_closure.PrintAllResolvableVariables()}])");
                }
            }

            if (_env.TryResolveRelation(identifier, out var relation)) {
                // Try to match the symbol to a semantic relation
                var node = new SemanticRelationQuery(
                    relation: relation,
                    terms: args
                ); // TODO { ParserContext = context };
                node.AssertCorrectness();
                return node;
            } else {
                // If the symbol doesn't refer to a semantic relation, assume it refers to a library function
                return new LibraryFunctionCall(
                    libraryFunction: _env.IncludeLibraryFunction(identifier),
                    arguments: args
                ); // TODO {ParserContext = context};
            }
        }
    }
}