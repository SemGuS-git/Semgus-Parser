using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    public abstract class SmtBinder : SmtTerm
    {
        public SmtBinder(SmtTerm child, SmtScope newScope) : base(child.Sort)
        {
            Child = child;
            NewScope = newScope;
        }

        public SmtTerm Child { get; }
        public SmtScope NewScope { get; }
    }
}
