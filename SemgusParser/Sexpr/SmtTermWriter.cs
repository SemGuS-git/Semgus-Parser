using Semgus.Model.Smt.Terms;
using Semgus.Sexpr.Writer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Sexpr
{
    internal class SmtTermWriter : ISmtTermVisitor<ISexprWriter>
    {
        private readonly ISexprWriter _sw;

        public SmtTermWriter(ISexprWriter sw)
        {
            _sw = sw;
        }

        /// <summary>
        /// Wraps an annotation block around the current term, if necessary
        /// </summary>
        /// <param name="term">Term to write annotations for, if any</param>
        /// <param name="inner">Action to call for writing the term</param>
        private void MaybeWriteAnnotations(SmtTerm term, Action inner)
        {
            if (term.Annotations != null && term.Annotations.Count > 0)
            {
                _sw.WriteList(() =>
                {
                    _sw.WriteSymbol("annotated");
                    inner();
                    foreach (var ann in term.Annotations)
                    {
                        _sw.WriteString(ann.Keyword.Name);
                        _sw.Write(ann.Value);
                    }
                });
            }
            else
            {
                inner();
            }
        }

        public ISexprWriter VisitBitVectorLiteral(SmtBitVectorLiteral bitVectorLiteral)
        {
            MaybeWriteAnnotations(bitVectorLiteral, () =>
            {
                _sw.WriteBitVector(bitVectorLiteral.Value);
            });
            return _sw;
        }

        public ISexprWriter VisitDecimalLiteral(SmtDecimalLiteral decimalLiteral)
        {
            MaybeWriteAnnotations(decimalLiteral, () =>
            {
                _sw.WriteDecimal(decimalLiteral.Value);
            });
            return _sw;
        }

        public ISexprWriter VisitExistsBinder(SmtExistsBinder existsBinder)
        {
            MaybeWriteAnnotations(existsBinder, () =>
            {
                _sw.WriteList(() =>
                {
                    _sw.WriteSymbol("exists");
                    _sw.WriteKeyword("bindings");
                    _sw.WriteList(existsBinder.NewScope.LocalBindings, b => _sw.Write(b.Id));
                    _sw.WriteKeyword("binding-sorts");
                    _sw.WriteList(existsBinder.NewScope.LocalBindings, b => _sw.Write(b.Sort.Name));
                    _sw.WriteKeyword("child");
                    existsBinder.Child.Accept(this);
                });
            });
            return _sw;
        }

        public ISexprWriter VisitForallBinder(SmtForallBinder forallBinder)
        {
            MaybeWriteAnnotations(forallBinder, () =>
            {
                _sw.WriteList(() =>
                {
                    _sw.WriteSymbol("forall");
                    _sw.WriteKeyword("bindings");
                    _sw.WriteList(forallBinder.NewScope.LocalBindings, b => _sw.Write(b.Id));
                    _sw.WriteKeyword("binding-sorts");
                    _sw.WriteList(forallBinder.NewScope.LocalBindings, b => _sw.Write(b.Sort.Name));
                    _sw.WriteKeyword("child");
                    forallBinder.Child.Accept(this);
                });
            });
            return _sw;
        }

        public ISexprWriter VisitFunctionApplication(SmtFunctionApplication functionApplication)
        {
            MaybeWriteAnnotations(functionApplication, () =>
            {
                _sw.WriteList(() =>
                {
                    _sw.WriteSymbol("application");
                    _sw.Write(functionApplication.Definition.Name);
                    _sw.WriteKeyword("argument-sorts");
                    _sw.WriteList(functionApplication.Rank.ArgumentSorts, s => _sw.Write(s.Name));
                    _sw.WriteKeyword("arguments");
                    _sw.WriteList(functionApplication.Arguments, a => a.Accept(this));
                    _sw.WriteKeyword("return-sort");
                    _sw.Write(functionApplication.Rank.ReturnSort.Name);
                });
            });
            return _sw;
        }

        public ISexprWriter VisitLambdaBinder(SmtLambdaBinder lambdaBinder)
        {
            MaybeWriteAnnotations(lambdaBinder, () =>
            {
                _sw.WriteList(() =>
                {
                    _sw.WriteSymbol("lambda");
                    _sw.WriteKeyword("arguments");
                    _sw.WriteList(lambdaBinder.ArgumentNames, an => _sw.Write(an));
                    _sw.WriteKeyword("body");
                    lambdaBinder.Child.Accept(this);
                });
            });
            return _sw;
        }

        public ISexprWriter VisitLetBinder(SmtLetBinder letBinder)
        {
            throw new NotImplementedException();
        }

        public ISexprWriter VisitMatchBinder(SmtMatchBinder matchBinder)
        {
            MaybeWriteAnnotations(matchBinder, () =>
            {
                _sw.WriteList(() =>
                {
                    _sw.WriteSymbol("binder");
                    _sw.WriteKeyword("operator");
                    if (matchBinder.Constructor is null)
                    {
                        _sw.WriteNil();
                    }
                    else
                    {
                        _sw.Write(matchBinder.Constructor.Name);
                    }
                    _sw.WriteKeyword("arguments");
                    _sw.WriteList(matchBinder.Bindings, b => _sw.Write(b.Binding.Id));
                    _sw.WriteKeyword("child");
                    matchBinder.Child.Accept(this);
                });
            });
            return _sw;
        }

        public ISexprWriter VisitMatchGrouper(SmtMatchGrouper matchGrouper)
        {
            MaybeWriteAnnotations(matchGrouper, () =>
            {
                _sw.WriteList(() =>
                {
                    _sw.WriteSymbol("match");
                    _sw.WriteKeyword("term");
                    matchGrouper.Term.Accept(this);
                    _sw.WriteKeyword("binders");
                    _sw.WriteList(matchGrouper.Binders, b => b.Accept(this));
                });
            });
            return _sw;
        }

        public ISexprWriter VisitNumeralLiteral(SmtNumeralLiteral numeralLiteral)
        {
            MaybeWriteAnnotations(numeralLiteral, () => { _sw.WriteNumeral(numeralLiteral.Value); });
            return _sw;
        }

        public ISexprWriter VisitStringLiteral(SmtStringLiteral stringLiteral)
        {
            MaybeWriteAnnotations(stringLiteral, () => { _sw.WriteString(stringLiteral.Value); });
            return _sw;
        }

        public ISexprWriter VisitVariable(SmtVariable variable)
        {
            MaybeWriteAnnotations(variable, () =>
            {
                _sw.WriteList(() =>
                {
                    _sw.WriteSymbol("variable");
                    _sw.Write(variable.Name);
                    _sw.WriteKeyword("sort");
                    _sw.Write(variable.Sort.Name);
                });
            });
            return _sw;
        }
    }
}
