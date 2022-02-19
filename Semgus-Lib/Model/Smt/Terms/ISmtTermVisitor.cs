using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    public interface ISmtTermVisitor<out TOutput>
    {
        public TOutput VisitVariable(SmtVariable variable);
        public TOutput VisitFunctionApplication(SmtFunctionApplication functionApplication);
        public TOutput VisitStringLiteral(SmtStringLiteral stringLiteral);
        public TOutput VisitNumeralLiteral(SmtNumeralLiteral numeralLiteral);
        public TOutput VisitDecimalLiteral(SmtDecimalLiteral decimalLiteral);
        public TOutput VisitExistsBinder(SmtExistsBinder existsBinder);
        public TOutput VisitForallBinder(SmtForallBinder forallBinder);
    }
}
