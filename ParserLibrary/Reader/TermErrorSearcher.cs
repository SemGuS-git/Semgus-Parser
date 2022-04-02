using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// Checks a term hierarchy for error terms
    /// </summary>
    internal class TermErrorSearcher : ISmtTermVisitor<bool>
    {
        public bool VisitExistsBinder(SmtExistsBinder existsBinder)
            => existsBinder.Sort is ErrorSort
            || existsBinder.Child is ErrorTerm
            || existsBinder.Child.Accept(this);

        public bool VisitForallBinder(SmtForallBinder forallBinder)
            => forallBinder.Sort is ErrorSort
            || forallBinder.Child is ErrorTerm
            || forallBinder.Child.Accept(this);

        public bool VisitFunctionApplication(SmtFunctionApplication functionApplication)
            => functionApplication.Sort is ErrorSort
            || functionApplication.Arguments.Any(arg => arg is ErrorTerm || arg.Accept(this));

        public bool VisitNumeralLiteral(SmtNumeralLiteral numeralLiteral)
            => false;

        public bool VisitDecimalLiteral(SmtDecimalLiteral decimalLiteral)
            => false;

        public bool VisitStringLiteral(SmtStringLiteral stringLiteral)
            => false;

        public bool VisitVariable(SmtVariable variable)
            => variable.Sort is ErrorSort;

        public bool VisitMatchGrouper(SmtMatchGrouper matchGrouper)
            => matchGrouper.Sort is ErrorSort
            || matchGrouper.Term is ErrorTerm
            || matchGrouper.Term.Accept(this)
            || matchGrouper.Binders.Any(mb => mb.Accept(this));

        public bool VisitMatchBinder(SmtMatchBinder matchBinder)
            => matchBinder.Sort is ErrorSort
            || matchBinder.Child is ErrorTerm
            || matchBinder.Child.Accept(this);

        public bool VisitLambdaBinder(SmtLambdaBinder lambdaBinder)
            => lambdaBinder.Sort is ErrorSort
            || lambdaBinder.Child is ErrorTerm
            || lambdaBinder.Child.Accept(this);

        public bool VisitLetBinder(SmtLetBinder letBinder)
            => letBinder.Sort is ErrorSort
            || letBinder.Child is ErrorTerm
            || letBinder.Child.Accept(this);
    }
}
