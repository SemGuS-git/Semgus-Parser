using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt.Terms;

namespace Semgus.Model.Smt
{
    public class SmtAnnotation
    {
        public SmtAnnotation(SmtKeyword keyword, SmtTerm value)
        {
            Keyword = keyword;
            Value = value;
        }

        public SmtKeyword Keyword { get; }
        public SmtTerm Value { get; }
    }
}
