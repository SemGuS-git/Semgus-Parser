using Semgus.Model.Smt.Terms;
using Semgus.Sexpr.Writer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Semgus
{
    internal class SemgusTermWriter : ISmtTermVisitor<ISexprWriter>
    {
        private readonly ISexprWriter _sw;

        public SemgusTermWriter(ISexprWriter sw)
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
                _sw.WriteSpace();
                _sw.WriteList(existsBinder.NewScope.LocalBindings, b =>
                {
                    _sw.WriteList(() =>
                    {
                        _sw.Write(b.Id);
                        _sw.WriteSpace();
                        _sw.Write(b.Sort.Name);
                    });
                    _sw.AddConditionalNewline();
                });
                _sw.LogicalBlockIndent(3);
                _sw.AddConditionalNewline();
                existsBinder.Child.Accept(this);
            });
            return _sw;
        }

        public ISexprWriter VisitForallBinder(SmtForallBinder forallBinder)
        {
            _sw.WriteList(() =>
            {
                _sw.WriteSymbol("forall");
                _sw.WriteSpace();
                _sw.WriteList(forallBinder.NewScope.LocalBindings, b =>
                {
                    _sw.WriteList(() =>
                    {
                        _sw.Write(b.Id);
                        _sw.WriteSpace();
                        _sw.Write(b.Sort.Name);
                    });
                    _sw.AddConditionalNewline();
                });
                _sw.LogicalBlockIndent(3);
                _sw.AddConditionalNewline();
                forallBinder.Child.Accept(this);
            });
            return _sw;
        }

        public ISexprWriter VisitFunctionApplication(SmtFunctionApplication functionApplication)
        {
            if (functionApplication.Arguments.Count > 0)
            {
                _sw.WriteList(() =>
                {
                    _sw.Write(functionApplication.Definition.Name);
                    _sw.LogicalBlockCurrentIndent(1);
                    _sw.WriteSpace();
                    _sw.Write(functionApplication.Arguments, arg =>
                    {
                        arg.Accept(this);
                    });
                });
            }
            else
            {
                _sw.Write(functionApplication.Definition.Name);
            }
            return _sw;
        }

        public ISexprWriter VisitLambdaBinder(SmtLambdaBinder lambdaBinder)
        {
            _sw.WriteList(() =>
            {
                _sw.WriteSymbol("lambda");
                _sw.WriteList(lambdaBinder.ArgumentNames, an => _sw.Write(an), insertNewlines: true);
                lambdaBinder.Child.Accept(this);
            });
            return _sw;
        }

        public ISexprWriter VisitLetBinder(SmtLetBinder letBinder)
        {
            throw new NotImplementedException();
        }

        public ISexprWriter VisitMatchBinder(SmtMatchBinder matchBinder)
        {
            _sw.WriteList(() =>
            {
                if (matchBinder.Constructor is null)
                {
                    _sw.WriteNil();
                }
                else if (matchBinder.Constructor.Children.Count > 0)
                {
                    _sw.WriteList(() =>
                    {
                        _sw.Write(matchBinder.Constructor.Name);
                        foreach (var b in matchBinder.Bindings)
                        {
                            _sw.Write(b.Binding.Id);
                        }
                    });
                }
                else
                {
                    _sw.Write(matchBinder.Constructor.Name);
                }
                matchBinder.Child.Accept(this);
            });
            return _sw;
        }

        public ISexprWriter VisitMatchGrouper(SmtMatchGrouper matchGrouper)
        {
            _sw.WriteList(() =>
            {
                _sw.WriteSymbol("match");
                matchGrouper.Term.Accept(this);
                _sw.LogicalBlockIndent(2);
                _sw.WriteList(matchGrouper.Binders, b => {
                    _sw.AddConditionalNewline();
                    b.Accept(this);
                });
            });
            return _sw;
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
            _sw.Write(variable.Name);
            return _sw;
        }
    }
}
