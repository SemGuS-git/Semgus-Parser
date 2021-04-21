using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using Semgus.Parser.Internal;

namespace Semgus.Syntax {
    /// <summary>
    /// Converts an ANTLR CST to an AST, performing name analysis and error checking in the process.
    /// TODO: what's a better name for this?
    /// </summary>
    public class SyntaxNormalizer {
        private LanguageEnvironment _env;
        private Stack<VariableClosure> _closures;

        private bool _closed = false;

        public (SemgusProblem, LanguageEnvironment) Normalize([NotNull] SemgusParser.StartContext context) {
            this._env = new LanguageEnvironmentCollector().Visit(context);
            this._closures = new Stack<VariableClosure>();

            var sfNode = ProcessSynthFun(context.synth_fun());

            _closures.Push(sfNode.Closure);

            var constraints = context.constraint().Select(ProcessConstraint).ToList();

            _closures.Pop();

            return (
                new SemgusProblem(
                    synthFun: sfNode,
                    constraints: constraints
                ) { ParserContext = context },
                _env
            );
        }

        private SynthFun ProcessSynthFun([NotNull] SemgusParser.Synth_funContext context) {
            context.input_args().var_decl_list();

            var closure = MakeVariableClosure(
                DeclareVariables(VariableDeclaration.Context.SF_Input, context.input_args().var_decl_list().var_decl()),
                DeclareVariables(VariableDeclaration.Context.SF_Output, context.output_args().var_decl_list().var_decl())
            );

            _closures.Push(closure);

            var sfNode = new SynthFun(

                name: context.symbol().GetText(),
                closure: closure,
                productions: context.productions().production().Select(ProcessProduction).ToList()
            ) { ParserContext = context };

            _closures.Pop();

            return sfNode;
        }

        private Constraint ProcessConstraint([NotNull] SemgusParser.ConstraintContext context) {
            var closure = MakeVariableClosure(new[]{
                // Temp: hardcode t:Term into constraint context as auxiliary variable
                new VariableDeclaration( name: "t", type: _env.ResolveType(NonterminalTermDeclaration.TYPE_NAME), VariableDeclaration.Context.CT_Term) { ParserContext = context}
            });
            _closures.Push(closure);

            var formula = new FormulaConverter(_env, closure).ConvertFormula(context.formula());

            _closures.Pop();

            return new Constraint(
                closure: closure,
                formula: formula
            ) { ParserContext = context };
        }

        private Production ProcessProduction([NotNull] SemgusParser.ProductionContext context) {
            var cst_lhs = context.production_lhs();
            var cst_rhs = context.production_rhs();
            var cst_rel = cst_lhs.nt_relation();
            var cst_rel_symbols = cst_rel.symbol();

            var nonterminal = _env.ResolveNonterminal(cst_lhs.nt_name().symbol());

            // TODO: relable aux variables as outputs when implied by their position in the relation
            var closure = MakeVariableClosure(
                DeclareVariables(VariableDeclaration.Context.NT_Auxiliary, cst_rel.var_decl_list().var_decl())
                  .Prepend(MakeProductionTermDeclaration(cst_lhs))
            );
            _closures.Push(closure);

            var relationInstance = MakeRelationInstance(cst_rel);

            var node = new Production(
                nonterminal: nonterminal,
                closure: closure,
                relationInstance: relationInstance,
                productionRules: cst_rhs.Select(ProcessProductionRule).ToList()
            ) { ParserContext = context };

            node.AssertCorrectness();

            _closures.Pop();

            return node;
        }

        private ProductionRule ProcessProductionRule([NotNull] SemgusParser.Production_rhsContext context) {
            var cst_expr = context.rhs_expression();
            var cst_pred = context.predicate();

            var choiceExpressionConverter = new ChoiceExpressionConverter(_env);
            var choiceExpression = choiceExpressionConverter.ProcessChoiceExpression(cst_expr);

            var closure = MakeVariableClosure(
                choiceExpressionConverter.DeclaredTerms,
                DeclareVariables(VariableDeclaration.Context.PR_Auxiliary, cst_pred.var_decl_list().var_decl()
            ));

            _closures.Push(closure);

            var node = new ProductionRule(
                rewriteExpression: choiceExpression,
                closure: closure,
                predicate: new FormulaConverter(_env, closure).ConvertFormula(cst_pred.formula())
            ) { ParserContext = context };

            _closures.Pop();
            return node;
        }

        private IEnumerable<VariableDeclaration> DeclareVariables(VariableDeclaration.Context usage, IEnumerable<SemgusParser.Var_declContext> contexts) {
            return contexts.Select(context => new VariableDeclaration(
                name: context.symbol().GetText(),
                type: _env.ResolveType(context.type()),
                declarationContext: usage
            ) { ParserContext = context });
        }

        private VariableClosure MakeVariableClosure(params IEnumerable<VariableDeclaration>[] varLists) {
            var parent = _closures.Count > 0 ? _closures.Peek() : null;
            return new VariableClosure(
                parent: parent,
                variables: varLists.SelectMany(v => v).ToList()
            );
        }

        private NonterminalTermDeclaration MakeProductionTermDeclaration(SemgusParser.Production_lhsContext context) {
            return new NonterminalTermDeclaration(
                name: context.nt_term().symbol().GetText(),
                type: _env.ResolveType(NonterminalTermDeclaration.TYPE_NAME),
                nonterminal: _env.ResolveNonterminal(context.nt_name().symbol()),
                declarationContext: VariableDeclaration.Context.NT_Term
            ) { ParserContext = context };
        }

        private SemanticRelationInstance MakeRelationInstance([NotNull] SemgusParser.Nt_relationContext context) {
            var symbols = context.symbol();
            var relationDef = _env.ResolveRelation(symbols[0]);

            var closure = _closures.Peek();

            var variables = symbols.Skip(1).Select(closure.Resolve).ToList();

            var node = new SemanticRelationInstance(
                relation: relationDef,
                elements: variables
            ) { ParserContext = context };

            node.AssertCorrectness();

            return node;
        }
    }
}