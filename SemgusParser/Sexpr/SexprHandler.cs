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

namespace Semgus.Parser.Sexpr
{
    internal class SexprHandler : ISemgusProblemHandler
    {
        private readonly ISexprWriter _sw;

        public SexprHandler(TextWriter writer)
        {
            _sw = new SexprWriter(writer);
        }

        public void OnCheckSynth(SmtContext smtCtx, SemgusContext semgusCtx)
        {
            foreach (var chc in semgusCtx.Chcs)
            {
                _sw.WriteList(() =>
                {
                    _sw.WriteSymbol("chc");
                    _sw.WriteKeyword("head");
                    _sw.Write(chc.Head);
                    _sw.WriteKeyword("body");
                    _sw.WriteList(chc.BodyRelations, b => _sw.Write(b));
                    if (chc.InputVariables is not null)
                    {
                        _sw.WriteKeyword("input-variables");
                        _sw.WriteList(chc.InputVariables, v => _sw.Write(v.Name));
                    }
                    if (chc.OutputVariables is not null)
                    {
                        _sw.WriteKeyword("output-variables");
                        _sw.WriteList(chc.OutputVariables, v => _sw.Write(v.Name));
                    }
                    _sw.WriteKeyword("variables");
                    _sw.WriteList(chc.VariableBindings, vb => _sw.Write(vb.Id));
                    _sw.WriteKeyword("symbols");
                    _sw.Write(chc.Symbols);
                    _sw.WriteKeyword("constraint");
                    _sw.Write(chc.Constraint);
                    _sw.WriteKeyword("constructor");
                    _sw.WriteConstructor(chc.Binder);
                });
            };

            foreach (var ssf in semgusCtx.SynthFuns)
            {
                _sw.Write(ssf);
            }

            foreach (var constraint in semgusCtx.Constraints)
            {
                _sw.WriteList(() =>
                {
                    _sw.WriteSymbol("constraint");
                    _sw.Write(constraint);
                });
            }

            _sw.WriteList(() =>
            {
                _sw.WriteSymbol("check-synth");
            });
        }

        public void OnConstraint(SmtContext smtCtx, SemgusContext semgusCxt, SmtTerm constraint)
        {
        }

        public void OnSetInfo(SmtContext ctx, SmtAttribute attr)
        {
            _sw.WriteList(() =>
            {
                _sw.WriteSymbol("set-info");
                _sw.WriteKeyword(attr.Keyword.Name);
                _sw.Write(attr.Value);
            });
        }

        public void OnSynthFun(SmtContext ctx, SmtIdentifier name, IList<(SmtIdentifier, SmtSortIdentifier)> args, SmtSort sort)
        {
        }

        public void OnTermTypes(IReadOnlyList<SemgusTermType> termTypes)
        {
            _sw.WriteList(() =>
            {
                _sw.WriteSymbol("declare-term-types");
                foreach (var tt in termTypes)
                {
                    _sw.Write(tt.Name);
                }
            });

            foreach (var tt in termTypes)
            {
                foreach (var cons in tt.Constructors)
                {
                    _sw.WriteList(() =>
                    {
                        _sw.WriteSymbol("add-constructor");
                        _sw.Write(tt.Name);
                        _sw.WriteKeyword("operator");
                        _sw.Write(cons.Operator);
                        _sw.WriteKeyword("children");
                        _sw.WriteList(cons.Children, c => _sw.Write(c.Name));
                    });
                }
            }
        }

        public void OnFunctionDeclaration(SmtContext ctx, SmtFunction function, SmtFunctionRank rank)
        {
            _sw.WriteList(() =>
            {
                _sw.WriteSymbol("declare-function");
                _sw.Write(function.Name);
                _sw.WriteKeyword("rank");
                _sw.Write(rank);
            });
        }

        public void OnFunctionDefinition(SmtContext ctx, SmtFunction function, SmtFunctionRank rank, SmtLambdaBinder lambda)
        {
            _sw.WriteList(() =>
            {
                _sw.WriteSymbol("define-function");
                _sw.Write(function.Name);
                _sw.WriteKeyword("rank");
                _sw.Write(rank);
                _sw.WriteKeyword("definition");
                _sw.Write(lambda);
            });
        }

        /// <summary>
        /// Called when datatypes are defined
        /// </summary>
        /// <param name="ctx">SMT context</param>
        /// <param name="datatype">Defined datatypes</param>
        public void OnDatatypes(SmtContext ctx, IEnumerable<SmtDatatype> datatypes)
        {
            foreach (var dt in datatypes)
            {
                _sw.WriteList(() =>
                {
                    _sw.WriteSymbol("declare-datatype");
                    _sw.Write(dt.Name);
                    _sw.WriteKeyword("arity");
                    _sw.WriteNumeral(dt.Arity);
                });
            }

            foreach (var dt in datatypes)
            {
                foreach (var cons in dt.Constructors)
                {
                    _sw.WriteList(() =>
                    {
                        _sw.WriteSymbol("add-datatype-constructor");
                        _sw.WriteKeyword("datatype");
                        _sw.Write(dt.Name);
                        _sw.WriteKeyword("name");
                        _sw.Write(cons.Name);
                        _sw.WriteKeyword("children");
                        _sw.WriteList(cons.Children, c => _sw.Write(c.Name));
                    });
                }
            }
        }
    }
}
