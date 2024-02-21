using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Sorts;
using Semgus.Model.Smt.Terms;

using Semgus.Sexpr.Writer;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Semgus
{
    public class SemgusHandler : ISemgusProblemHandler
    {
        private readonly ISexprWriter _sw;

        public SemgusHandler(TextWriter writer)
        {
            _sw = new SexprWriter(writer, true, autoDelimit: false);
        }

        public void OnCheckSynth(SmtContext smtCtx, SemgusContext semgusCtx)
        {
            var byHead = semgusCtx.Chcs.GroupBy(chc => chc.Head);
            _sw.WriteList(() =>
            {
                _sw.WriteSymbol("define-funs-rec");
                _sw.WriteSpace();
                _sw.AddConditionalNewline();

                // CHC heads
                _sw.WriteList(byHead, grouping =>
                {
                    _sw.WriteList(() =>
                    {
                        _sw.Write(grouping.Key.Relation.Name);
                        _sw.WriteSpace();
                        _sw.WriteList(grouping.Key.Arguments, a =>
                        {
                            _sw.WriteList(() =>
                            {
                                _sw.Write(a.Name);
                                _sw.WriteSpace();
                                _sw.Write(a.Sort.Name);
                            });
                        });
                    });
                }, insertNewlines: true);
                _sw.WriteSpace();
                _sw.AddConditionalNewline();

                // CHC bodies
                _sw.WriteList(byHead, grouping =>
                {
                    _sw.AddConditionalNewline();
                    var rep = grouping.First();

                    // Annotations
                    _sw.WriteList(() =>
                    {
                        _sw.WriteSymbol("!");
                        _sw.WriteSpace();
                        _sw.WriteList(() =>
                        {
                            _sw.WriteSymbol("match");
                            _sw.WriteSpace();
                            _sw.Write(rep.Head.Arguments[0]); // Hack. Actually store the term
                            _sw.WriteSpace();
                            _sw.AddConditionalNewline();
                            _sw.WriteList(grouping.GroupBy(g => g.Binder.Constructor), chcs =>
                            {
                                _sw.AddConditionalNewline();
                                _sw.WriteList(() =>
                                {
                                    if (chcs.Key is null)
                                    {
                                        // TODO
                                        return;
                                    }

                                    if (chcs.Key.Children.Count > 0)
                                    {
                                        _sw.WriteList(() =>
                                        {
                                            _sw.Write(chcs.Key.Name);
                                            _sw.WriteSpace();
                                            _sw.Write(chcs.First().Binder.Bindings, b =>
                                            {
                                                _sw.Write(b.Binding.Id);
                                            });
                                        });
                                    }
                                    else
                                    {
                                        _sw.Write(chcs.Key.Name);
                                    }

                                    _sw.WriteSpace();
                                    _sw.AddConditionalNewline();
                                    _sw.WriteList(() =>
                                    {
                                        _sw.WriteSymbol("or");
                                        _sw.WriteSpace();
                                        _sw.Write(chcs, chc =>
                                        {
                                            _sw.Write(chc.Constraint);
                                        });
                                    });
                                });
                            });
                        });
                        if (rep.InputVariables is not null)
                        {
                            _sw.WriteSpace();
                            _sw.AddConditionalNewline();
                            _sw.WriteKeyword("input");
                            _sw.WriteSpace();
                            _sw.WriteList(rep.InputVariables, v => _sw.Write(v));
                        }
                        if (rep.OutputVariables is not null)
                        {
                            _sw.WriteSpace();
                            _sw.AddConditionalNewline();
                            _sw.WriteKeyword("output");
                            _sw.WriteSpace();
                            _sw.WriteList(rep.OutputVariables, v => _sw.Write(v));
                        }
                    });
                });
            });

            _sw.WriteList(() => _sw.WriteSymbol("check-synth"));
        }

        public void OnConstraint(SmtContext smtCtx, SemgusContext semgusCxt, SmtTerm constraint)
        {
            _sw.WriteList(() =>
            {
                _sw.WriteSymbol("constraint");
                _sw.WriteSpace();
                _sw.Write(constraint);
            });
        }

        public void OnDatatypes(SmtContext ctx, IEnumerable<SmtDatatype> datatypes)
        {
            //throw new NotImplementedException();
        }

        public void OnFunctionDeclaration(SmtContext ctx, SmtFunction function, SmtFunctionRank rank)
        {
            //throw new NotImplementedException();
        }

        public void OnFunctionDefinition(SmtContext ctx, SmtFunction function, SmtFunctionRank rank, SmtLambdaBinder lambda)
        {
            //throw new NotImplementedException();
        }

        public void OnSetInfo(SmtContext ctx, SmtAttribute attr)
        {
            _sw.WriteList(() =>
            {
                _sw.WriteSymbol("set-info");
                _sw.Write(attr.Keyword);
                _sw.Write(attr.Value);
            });
        }

        public void OnSynthFun(SmtContext ctx, SmtIdentifier name, IList<(SmtIdentifier, SmtSortIdentifier)> args, SmtSort sort)
        {
            _sw.WriteList(() =>
            {
                _sw.WriteSymbol("synth-fun");
                _sw.WriteSpace();
                _sw.Write(name);
                _sw.WriteSpace();
                _sw.WriteList(() => { }); // Should always be empty
                _sw.WriteSpace();
                _sw.Write(sort.Name);
            });
        }

        public void OnTermTypes(IReadOnlyList<SemgusTermType> termTypes)
        {
            _sw.WriteList(() =>
            {
                _sw.WriteSymbol("declare-term-types");

                // Declaration and arity
                _sw.AddConditionalNewline();
                _sw.WriteList(termTypes, tt =>
                {
                    _sw.WriteList(() =>
                    {
                        _sw.Write(tt.Name);
                        _sw.WriteSpace();
                        _sw.Write(tt.Arity);
                    });
                });

                // Constructors
                _sw.AddConditionalNewline();
                _sw.WriteList(termTypes, tt =>
                {
                    _sw.AddConditionalNewline();
                    _sw.WriteList(tt.Constructors, cons =>
                    {
                        _sw.AddConditionalNewline();
                        if (cons.Children.Length > 0)
                        {
                            _sw.WriteList(() =>
                            {
                                _sw.Write(cons.Operator);
                                foreach (var childSort in cons.Children)
                                {
                                    _sw.WriteSpace();
                                    _sw.Write(childSort.Name);
                                }
                            });
                        }
                        else
                        {
                            _sw.Write(cons.Operator);
                        }
                    });
                });
            });
        }
    }
}
