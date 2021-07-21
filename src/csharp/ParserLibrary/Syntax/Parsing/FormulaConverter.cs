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
        private readonly LanguageEnvironment _env;
        private readonly VariableClosure _closure;

        public FormulaConverter(LanguageEnvironment env, VariableClosure closure) {
            _env = env;
            _closure = closure;
        }

        public IFormula ConvertFormula(FormulaForm form) {
            if (form.Atom is not null) return ConvertFormulaAtom(form.Atom);
            if (form.List is not null) return ConvertFormulaList(form);

            // TODO throw new SemgusSyntaxException(context, "Invalid formula");
            throw new InvalidOperationException("Invalid formula");
        }

        private IFormula ConvertFormulaList(FormulaForm form) {
            if (form.List.Count == 0) {
                // This shouldn't happen
                throw new InvalidOperationException("Empty formula encountered");
            }

            // Process list as an invocation of the first entry (as a library function) on the subsequent entries

            var subformulae = form.List.Select(ConvertFormula);
            var arguments = subformulae.Pop(out var head);

            if (head is LibraryDefinedSymbol placeholder) {
                return MakeInvocation(form.Position, placeholder.Identifier, arguments.ToList());
            } else {
                throw new InvalidOperationException("Expected symbol at head of formula, but got: " + head);
            }
        }

        private IFormula ConvertFormulaAtom(SemgusToken atom) {
            if (atom is SymbolToken symbol) {
                var id = symbol.Name;

                if (_closure.TryResolve(id, out var variable)) {
                    // Resolve the symbol to a variable if possible
                    return new VariableEvaluation(variable); // TODO { ParserContext = context };
                } else {
                    // Otherwise, expect to find it in our library
                    return new LibraryDefinedSymbol(id); // TODO { ParserContext = context };
                }
            } else if (atom.IsLiteral) {
                return Literal.Convert(atom);
            } else {
                throw new InvalidOperationException("Non-literal, non-symbol atom encountered in formula context: " + atom);
            }
        }

        private IFormula MakeInvocation(SemgusParserContext context, string identifier, List<IFormula> args) {
            for (int i = 0; i < args.Count; i++) {
                if(args[i] is LibraryDefinedSymbol ph) args[i] = new LibraryDefinedSymbol(ph.Identifier);
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