using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Semgus.Parser.Internal;

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
            public ParserRuleContext ParserContext { get; }
            public string Identifier { get; }

            public SymbolicPlaceholder(ParserRuleContext parserContext, string identifier) {
                ParserContext = parserContext;
                Identifier = identifier;
            }

            // this should never be visited
            public T Accept<T>(IAstVisitor<T> visitor) => throw new InvalidOperationException();
        }

        private readonly LanguageEnvironment _env;
        private readonly VariableClosure _closure;

        public FormulaConverter(LanguageEnvironment env, VariableClosure closure) {
            this._env = env;
            this._closure = closure;
        }

        public IFormula ConvertFormula([NotNull] SemgusParser.FormulaContext context) {
            var result = ConvertFormulaInner(context);
            if (result is SymbolicPlaceholder placeholder) {
                throw new Exception(); // probably an unbound variable
            }
            return result;
        }

        private IFormula ConvertFormulaInner([NotNull] SemgusParser.FormulaContext context) {
            if (context.literal() is SemgusParser.LiteralContext literalContext) {
                return Literal.Convert(literalContext);
            }

            if (context.symbol() is SemgusParser.SymbolContext symbolContext) {
                var id = symbolContext.GetText();
                if (_closure.TryResolve(id, out var variable)) {
                    return new VariableEvaluation(context, variable);
                } else {
                    return new SymbolicPlaceholder(context, id);
                }
            }

            var cst_subformulas = context.formula();

            if (cst_subformulas.Length == 0) throw new SemgusSyntaxException(context, "Empty formula");

            if (cst_subformulas.Length == 1) return ConvertFormulaInner(cst_subformulas[0]);

            // Application
            var subformulas = cst_subformulas.Select<SemgusParser.FormulaContext, IFormula>(ConvertFormulaInner).ToList();
            if (subformulas[0] is SymbolicPlaceholder placeholder) {
                return MakeInvocation(context, placeholder.Identifier, subformulas.Skip(1).ToList());
            }

            throw new SemgusSyntaxException(context, "Invalid formula");
        }

        private IFormula MakeInvocation(ParserRuleContext context, string identifier, List<IFormula> args) {
            foreach (var arg in args) {
                if (arg is SymbolicPlaceholder ph) throw new SemgusSyntaxException(ph.ParserContext, $"Unknown identifier \"{ph.Identifier}\" (not a variable in [{_closure.PrintAllResolvableVariables()}])");
            }

            if (_env.TryResolveRelation(identifier, out var relation)) {
                // Try to match the symbol to a semantic relation
                return new SemanticRelationQuery(
                    parserContext: context,
                    relation: relation,
                    terms: args
                );
            } else {
                // If the symbol doesn't refer to a semantic relation, assume it refers to a library function
                return new LibraryFunctionCall(
                    parserContext: context,
                    libraryFunction: _env.IncludeLibraryFunction(identifier),
                    arguments: args
                );
            }
        }
    }
}