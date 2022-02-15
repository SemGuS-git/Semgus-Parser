using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    public class SmtLambdaBinder : SmtBinder
    {
        public IReadOnlyList<SmtIdentifier> ArgumentNames { get; }

        public SmtLambdaBinder(SmtTerm child, SmtScope newScope, IEnumerable<SmtIdentifier> argNames) : base(child, newScope)
        {
            ArgumentNames = argNames.ToList();
        }
    }
}
