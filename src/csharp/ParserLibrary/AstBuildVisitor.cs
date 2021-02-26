using System;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Semgus.Parser.Internal;

namespace Semgus.Parser {
    public class AstException : Exception {
        public AstException(string message) : base(message) {
        }
    }
    
    // Converts CST nodes into AST nodes
    public class BuildAstVisitor : SemgusBaseVisitor<NodeBase> {
        public override NodeBase VisitStart([NotNull] SemgusParser.StartContext context) {
            return new StartNode(
              synthFun: Visit(context.synth_fun()),
              constraints: VisitEach(context.constraint())  
            );
        }
        
        public override NodeBase VisitSynth_fun([NotNull] SemgusParser.Synth_funContext context) {
            return new SynthFunNode(
                symbol: Visit(context.symbol()),
                inputArgs: VisitEach(context.input_args().var_decl_list().var_decl()),
                outputArgs: VisitEach(context.output_args().var_decl_list().var_decl()),
                productions: VisitEach(context.productions().production())
            );
        }
        

        public override NodeBase VisitProduction([NotNull] SemgusParser.ProductionContext context) {
            return new ProductionNode(
                lhs: Visit(context.production_lhs()),
                rhs: VisitEach(context.production_rhs())
            );
        }
        
        /* Production LHS */
        public override NodeBase VisitProduction_lhs([NotNull] SemgusParser.Production_lhsContext context) {
            return new ProductionLhsNode(
                ntName: Visit(context.nt_name()),
                ntRelationDef: Visit(context.nt_relation_def()),
                ntTerm: Visit(context.nt_term()),
                ntRelation: Visit(context.nt_relation())
            );
        }

        public override NodeBase VisitNt_relation_def([NotNull] SemgusParser.Nt_relation_defContext context) {
            var symbols = context.symbol();
            return new NtRelationDefNode(
                semanticRelationName: Visit(symbols[0]),
                varTypes: VisitEach(symbols.Skip(1).ToArray())
            );
        }

        public override NodeBase VisitNt_relation([NotNull] SemgusParser.Nt_relationContext context) {
            var symbols = context.symbol();
            return new NtRelationNode (
                auxVariableDefinitions: VisitEach(context.var_decl_list().var_decl()),
                semanticRelationName: Visit(symbols[0]),
                varNames: VisitEach(symbols.Skip(1).ToArray())
            );
        }
        
        /* Production RHS */
        public override NodeBase VisitProduction_rhs([NotNull] SemgusParser.Production_rhsContext context) {
            return new ProductionRhsNode(
                expression: Visit(context.rhs_expression()),
                predicates: Visit(context.predicate())
            );
        }

        public override NodeBase VisitRhs_expression([NotNull] SemgusParser.Rhs_expressionContext context) {
            var atoms = context.rhs_atom();
            if(context.op() is SemgusParser.OpContext opContext) {
                return new RhsOpExpressionNode(
                    op: Visit(opContext),
                    terms: VisitEach(atoms)
                );
            } else {
                if(atoms.Length != 1) throw new ArgumentException();
                return Visit(context.rhs_atom()[0]);
            }
        }

        public override NodeBase VisitRhs_atom([NotNull] SemgusParser.Rhs_atomContext context) {
            if(context.nt_name() is SemgusParser.Nt_nameContext nt_nameContext) {
                return new RhsNtAtomNode(
                    ntName: Visit(nt_nameContext),
                    ntTerm: Visit(context.nt_term())
                );
            }
            if(context.literal() is SemgusParser.LiteralContext literalContext) {
                return Visit(literalContext);
            }
            if(context.symbol() is SemgusParser.SymbolContext symbolContext) {
                return Visit(symbolContext);
            }
            throw new NotSupportedException();
        }

        
        public override NodeBase VisitPredicate([NotNull] SemgusParser.PredicateContext context) {
            return new PredicateNode(
                auxVariableDefinitions: VisitEach(context.var_decl_list().var_decl()),
                formula: Visit(context.formula())
            );
        }

        public override NodeBase VisitVar_decl([NotNull] SemgusParser.Var_declContext context) {
            return new VarDeclNode(Visit(context.symbol()), Visit(context.type()));
        }

        /* Formulas */
        private FunctionApplicationNode MakeFunctionApplication(SemgusParser.FormulaContext[] items) {
            var name = VisitFormula(items[0]);
            var args = new NodeBase[items.Length - 1];

            for (int i = 0; i < args.Length; i++) {
                args[i] = VisitFormula(items[i + 1]);
            }

            if (name is SymbolNode symbol) {
                return new FunctionApplicationNode(symbol, args);
            } else {
                throw new AstException("The first subformula in a function application must be a symbol");
            }
        }

        public override NodeBase VisitFormula([NotNull] SemgusParser.FormulaContext context) {
            if (context.symbol() == null && context.literal() == null) {
                var ff = context.formula();

                if (ff.Length == 0) return new EmptyFormulaNode();
                if (ff.Length == 1) return VisitFormula(ff[0]);
                if (ff.Length > 1) return MakeFunctionApplication(ff);
            }

            return base.VisitFormula(context);
        }

        /* Literals */
        public override NodeBase VisitIntConst([NotNull] SemgusParser.IntConstContext context) {
            return new LiteralNode<int>(int.Parse(context.GetText()));
        }

        public override NodeBase VisitBoolConst([NotNull] SemgusParser.BoolConstContext context) {
            return new LiteralNode<bool>(bool.Parse(context.GetText()));
        }

        public override NodeBase VisitBVConst([NotNull] SemgusParser.BVConstContext context) {
            throw new NotImplementedException();
        }

        public override NodeBase VisitEnumConst([NotNull] SemgusParser.EnumConstContext context) {
            throw new NotImplementedException();
        }

        public override NodeBase VisitRealConst([NotNull] SemgusParser.RealConstContext context) {
            return new LiteralNode<double>(double.Parse(context.GetText()));
        }

        public override NodeBase VisitQuotedLit([NotNull] SemgusParser.QuotedLitContext context) {
            if (context.DOUBLEQUOTEDLIT() != null) {
                return new LiteralNode<string>(Regex.Match(context.GetText(), "\"([^\"]*)\"").Captures[0].Value);
            }
            if (context.SINGLEQUOTEDLIT() != null) {
                return new LiteralNode<string>(Regex.Match(context.GetText(), "'([^']*)'").Captures[0].Value);
            }

            throw new ArgumentException();
        }

        /* Symbols */
        public override NodeBase VisitSymbol([NotNull] SemgusParser.SymbolContext context) {
            return new SymbolNode(value: context.GetText());
        }

        public override NodeBase VisitType([NotNull] SemgusParser.TypeContext context) {
            return new TypeNode(name: context.GetText());
        }

        /* Constraints */
        public override NodeBase VisitConstraint([NotNull] SemgusParser.ConstraintContext context) {
            return Visit(context.formula());
        }
        
        /* Utility */
        private NodeBase[] VisitEach(ParserRuleContext[] items) {
            var nodes = new NodeBase[items.Length];
            for (int i = 0; i < items.Length; i++) {
                nodes[i] = Visit(items[i]);
            }
            return nodes;
        }
    }

}
