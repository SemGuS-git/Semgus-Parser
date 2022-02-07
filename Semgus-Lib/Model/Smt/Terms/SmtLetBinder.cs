using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    public class SmtLetBinder : SmtBinder
    {
        public SmtLetBinder(SmtTerm child, SmtScope newScope) : base(child, newScope) { }
    }
}
