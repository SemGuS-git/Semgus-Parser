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

        public ISexprWriter VisitBitVectorLiteral(SmtBitVectorLiteral bitVectorLiteral)
        {
            _sw.WriteBitVector(bitVectorLiteral.Value);
            return _sw;
        }

        public ISexprWriter VisitDecimalLiteral(SmtDecimalLiteral decimalLiteral)
        {
            _sw.WriteDecimal(decimalLiteral.Value);
            return _sw;
        }

        public ISexprWriter VisitExistsBinder(SmtExistsBinder existsBinder)
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
            return _sw;
        }

        public ISexprWriter VisitForallBinder(SmtForallBinder forallBinder)
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
            return _sw;
        }

        public ISexprWriter VisitFunctionApplication(SmtFunctionApplication functionApplication)
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
            return _sw;
        }

        public ISexprWriter VisitLambdaBinder(SmtLambdaBinder lambdaBinder)
        {
            throw new NotImplementedException();
        }

        public ISexprWriter VisitLetBinder(SmtLetBinder letBinder)
        {
            throw new NotImplementedException();
        }

        public ISexprWriter VisitMatchBinder(SmtMatchBinder matchBinder)
        {
            throw new NotImplementedException();
        }

        public ISexprWriter VisitMatchGrouper(SmtMatchGrouper matchGrouper)
        {
            throw new NotImplementedException();
        }

        public ISexprWriter VisitNumeralLiteral(SmtNumeralLiteral numeralLiteral)
        {
            _sw.WriteNumeral(numeralLiteral.Value);
            return _sw;
        }

        public ISexprWriter VisitStringLiteral(SmtStringLiteral stringLiteral)
        {
            _sw.WriteString(stringLiteral.Value);
            return _sw;
        }

        public ISexprWriter VisitVariable(SmtVariable variable)
        {
            _sw.WriteList(() =>
            {
                _sw.WriteSymbol("variable");
                _sw.Write(variable.Name);
                _sw.WriteKeyword("sort");
                _sw.Write(variable.Sort.Name);
            });
            return _sw;
        }
    }
}
