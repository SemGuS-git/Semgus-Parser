using System.Linq;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Semgus.Parser.Internal;

namespace Semgus.Parser {
    // Prints out all of the CST nodes that it visits (for use in debugging)
    public class NaivePrintCstVisitor : SemgusBaseVisitor<string> {
        private int _indentLevel = 0;

        void Log(string s) {
            System.Console.WriteLine(string.Concat(Enumerable.Repeat("  ", _indentLevel)) + s);
        }

        public override string VisitBoolConst([NotNull] SemgusParser.BoolConstContext context) {
            Log("BoolConst");
            return base.VisitBoolConst(context);
        }

        public override string VisitBVConst([NotNull] SemgusParser.BVConstContext context) {
            Log("BVConst");
            return base.VisitBVConst(context);
        }

        public override string VisitChildren(IRuleNode node) {
            _indentLevel++;
            var res = base.VisitChildren(node);
            _indentLevel--;
            return res;
        }

        public override string VisitConstraint([NotNull] SemgusParser.ConstraintContext context) {
            Log("Constraint");
            return base.VisitConstraint(context);
        }

        public override string VisitEnumConst([NotNull] SemgusParser.EnumConstContext context) {
            Log("EnumConst");
            return base.VisitEnumConst(context);
        }

        public override string VisitErrorNode(IErrorNode node) {
            Log("ErrorNode");
            return base.VisitErrorNode(node);
        }

        public override string VisitFormula([NotNull] SemgusParser.FormulaContext context) {
            Log($"Formula: {context.GetText()}");
            return base.VisitFormula(context);
        }

        public override string VisitInput_args([NotNull] SemgusParser.Input_argsContext context) {
            Log("Input_args");
            return base.VisitInput_args(context);
        }

        public override string VisitIntConst([NotNull] SemgusParser.IntConstContext context) {
            Log("IntConst");
            return base.VisitIntConst(context);
        }

        public override string VisitLiteral([NotNull] SemgusParser.LiteralContext context) {
            Log($"Literal: {context.GetText()}");
            return base.VisitLiteral(context);
        }

        public override string VisitNt_name([NotNull] SemgusParser.Nt_nameContext context) {
            Log("Nt_name");
            return base.VisitNt_name(context);
        }

        public override string VisitNt_relation([NotNull] SemgusParser.Nt_relationContext context) {
            Log("Nt_relation");
            return base.VisitNt_relation(context);
        }

        public override string VisitNt_relation_def([NotNull] SemgusParser.Nt_relation_defContext context) {
            Log("Nt_relation_def");
            return base.VisitNt_relation_def(context);
        }

        public override string VisitNt_term([NotNull] SemgusParser.Nt_termContext context) {
            Log("Nt_term");
            return base.VisitNt_term(context);
        }

        public override string VisitOp([NotNull] SemgusParser.OpContext context) {
            Log("Op");
            return base.VisitOp(context);
        }

        public override string VisitOutput_args([NotNull] SemgusParser.Output_argsContext context) {
            Log("Output_args");
            return base.VisitOutput_args(context);
        }

        public override string VisitPredicate([NotNull] SemgusParser.PredicateContext context) {
            Log("Predicate");
            return base.VisitPredicate(context);
        }

        public override string VisitProduction([NotNull] SemgusParser.ProductionContext context) {
            Log("Production");
            return base.VisitProduction(context);
        }

        public override string VisitProductions([NotNull] SemgusParser.ProductionsContext context) {
            Log("Productions");
            return base.VisitProductions(context);
        }

        public override string VisitProduction_lhs([NotNull] SemgusParser.Production_lhsContext context) {
            Log("Production_lhs");
            return base.VisitProduction_lhs(context);
        }

        public override string VisitProduction_rhs([NotNull] SemgusParser.Production_rhsContext context) {
            Log("Production_rhs");
            return base.VisitProduction_rhs(context);
        }

        public override string VisitQuotedLit([NotNull] SemgusParser.QuotedLitContext context) {
            Log("QuotedLit");
            return base.VisitQuotedLit(context);
        }

        public override string VisitRealConst([NotNull] SemgusParser.RealConstContext context) {
            Log("RealConst");
            return base.VisitRealConst(context);
        }

        public override string VisitRhs_atom([NotNull] SemgusParser.Rhs_atomContext context) {
            Log("Rhs_atom");
            return base.VisitRhs_atom(context);
        }

        public override string VisitRhs_expression([NotNull] SemgusParser.Rhs_expressionContext context) {
            Log("Rhs_expression");
            return base.VisitRhs_expression(context);
        }

        public override string VisitStart([NotNull] SemgusParser.StartContext context) {
            Log("Start");
            return base.VisitStart(context);
        }

        public override string VisitSymbol([NotNull] SemgusParser.SymbolContext context) {
            Log($"Symbol: {context.GetText()}");
            return base.VisitSymbol(context);

        }

        public override string VisitSynth_fun([NotNull] SemgusParser.Synth_funContext context) {
            Log("Synth_fun");
            return base.VisitSynth_fun(context);
        }

        public override string VisitTerminal(ITerminalNode node) {
            Log($"T: {node.Symbol.Text}");
            return base.VisitTerminal(node);
        }

        public override string VisitType([NotNull] SemgusParser.TypeContext context) {
            Log("Type");
            return base.VisitType(context);
        }

        public override string VisitVar_decl([NotNull] SemgusParser.Var_declContext context) {
            Log("Var_decl");
            return base.VisitVar_decl(context);
        }

        public override string VisitVar_decl_list([NotNull] SemgusParser.Var_decl_listContext context) {
            Log("Var_decl_list");
            return base.VisitVar_decl_list(context);
        }
    }
}
