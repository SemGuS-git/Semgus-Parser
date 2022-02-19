using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    public class SmtExistsBinder : SmtBinder
    {
        public SmtExistsBinder(SmtTerm child, SmtScope newScope) : base(child, newScope) { }

        public override string ToString()
        {
            return $"(exists ({string.Join(' ', NewScope.LocalBindings.Select(b => $"({b.Id} {b.Sort.Name})"))}) {Child})";
        }

        public override TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor)
        {
            return visitor.VisitExistsBinder(this);
        }
    }
}
