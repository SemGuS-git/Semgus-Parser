using System.Collections.Generic;
using System.Linq;
using Semgus.Util;

namespace Semgus.Syntax {
    public class AstPrinter {
        public string PrettyPrint(ISyntaxNode node) {
            var visitor = new Visitor();
            return node.Accept(new Visitor()).ToString();
        }
        
        private class Visitor : IAstVisitor<CodeTextBuilder> {
            private readonly CodeTextBuilder _builder = new CodeTextBuilder();

            private CodeTextBuilder DoVisit(ISyntaxNode node) => node.Accept(this);

            private CodeTextBuilder VisitEach(IEnumerable<ISyntaxNode> nodes) {
                foreach (var node in nodes) DoVisit(node);
                return _builder;
            }

            private CodeTextBuilder VisitEach(IEnumerable<ISyntaxNode> nodes, string sep = " ") {
                bool first = true;
                foreach (var node in nodes) {
                    if (first) {
                        first = false;
                    } else {
                        _builder.Write(sep);
                    }
                    DoVisit(node);
                }
                return _builder;
            }

            private void PrintVariableClosure(VariableClosure closure) {
                using (_builder.InDelimiters("(", ")")) {
                    PrintVariableDecGroup("in", closure.Input());
                    PrintVariableDecGroup("out", closure.Output());
                    PrintVariableDecGroup("aux", closure.Auxiliary());
                }
            }

            private void PrintVariableDecGroup(string label, IEnumerable<VariableDeclaration> vardecs) {
                var list = vardecs.ToList();
                if (list.Count == 0) return;
                using (_builder.InParens()) {
                    _builder.Write(label);
                    _builder.Write(" ");
                    VisitEach(vardecs, " ");
                }
            }

            public CodeTextBuilder Visit(Constraint node) {
                _builder.LineBreak();
                using (_builder.InParens()) {
                    _builder.Write("constraint");
                    using (_builder.InLineBreaks()) {
                        return DoVisit(node.Formula);
                    }
                }
            }

            private static IEnumerable<T> Just<T>(T value) {
                yield return value;
            }

            public CodeTextBuilder Visit(AtomicRewriteExpression node) => DoVisit(node.Atom);

            public CodeTextBuilder Visit(LeafTerm node) => _builder.Write(node.Text);

            public CodeTextBuilder Visit(LibraryFunctionCall node) {
                using (_builder.InParens()) {
                    _builder.Write(node.LibraryFunction.Name);
                    _builder.Write(" ");
                    return VisitEach(node.Arguments, " ");
                }
            }

            public CodeTextBuilder Visit<TValue>(Literal<TValue> node) => _builder.Write(node.Value.ToString());

            public CodeTextBuilder Visit(NonterminalTermDeclaration node) => _builder.Write($"{node.Name}:{node.Nonterminal.Name}");

            public CodeTextBuilder Visit(VariableEvaluation node) => _builder.Write(node.Variable.Name);

            public CodeTextBuilder Visit(Operator node) => _builder.Write(node.Text);

            public CodeTextBuilder Visit(SemgusProblem node) => VisitEach(Just<ISyntaxNode>(node.SynthFun).Concat(node.Constraints));

            public CodeTextBuilder Visit(VariableDeclaration node) => _builder.Write($"{node.Name}:{node.Type}");

            public CodeTextBuilder Visit(SynthFun node) {
                using (_builder.InParens()) {
                    _builder.Write("synth-fun ");
                    _builder.Write(node.Name);
                    PrintVariableClosure(node.Closure);
                    using (_builder.InLineBreaks()) {
                        VisitEach(node.Productions);
                    }
                }
                _builder.LineBreak();
                return _builder;
            }

            public CodeTextBuilder Visit(SemanticRelationDeclaration node) {
                using (_builder.InParens()) {
                    _builder.WriteEach(Just(node.Name).Concat(node.ElementTypes.Select(e => e.Name)), " ");
                }
                return _builder;
            }

            public CodeTextBuilder Visit(SemanticRelationInstance node) {
                using (_builder.InParens()) {
                    _builder.WriteEach(Just(node.Relation.Name).Concat(node.Assignments.Select(v => v.Name)), " ");
                }
                return _builder;
            }

            public CodeTextBuilder Visit(SemanticRelationQuery node) {
                using (_builder.InParens()) {
                    _builder.Write(node.Relation.Name);
                    _builder.Write(" ");
                    VisitEach(node.Terms, " ");
                }
                return _builder;
            }

            public CodeTextBuilder Visit(OpRewriteExpression node) {
                using (_builder.InParens()) {
                    VisitEach(Just<ISyntaxNode>(node.Op).Concat(node.Operands), " ");
                }
                return _builder;
            }

            public CodeTextBuilder Visit(Production node) {
                using (_builder.InParens())
                using (_builder.InLineBreaks()) {
                    _builder.LineBreak();
                    _builder.Write(node.Nonterminal.Name);
                    _builder.Write(" : ");
                    DoVisit(node.RelationInstance.Relation);
                    _builder.Write(" : ");
                    using (_builder.InBrackets()) {
                        _builder.LineBreak();
                        PrintVariableClosure(node.Closure);
                        _builder.LineBreak();
                        using (_builder.InParens()) {
                            VisitEach(node.ProductionRules);
                        }
                    }
                }
                return _builder;
            }

            public CodeTextBuilder Visit(ProductionRule node) {
                using (_builder.InLineBreaks()) {
                    DoVisit(node.RewriteExpression);
                    _builder.Write(" ");
                    using (_builder.InBrackets()) {
                        _builder.LineBreak();
                        PrintVariableClosure(node.Closure);
                        _builder.LineBreak();
                        DoVisit(node.Predicate);
                        _builder.LineBreak();
                    }
                    return _builder;
                }
            }
        }
    }
}