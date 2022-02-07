using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    public abstract class SmtTerm
    {
        public SmtTerm(SmtSort sort)
        {
            Sort = sort;
        }

        public ISet<SmtAnnotation>? Annotations { get; }

        public SmtSort Sort { get; }
    }
}
