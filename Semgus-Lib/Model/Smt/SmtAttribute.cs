using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt.Terms;

namespace Semgus.Model.Smt
{
    public class SmtAttribute
    {
        public SmtAttribute(SmtKeyword keyword, SmtAttributeValue value)
        {
            Keyword = keyword;
            Value = value;
        }

        public SmtKeyword Keyword { get; }
        public SmtAttributeValue Value { get; }
    }
}
