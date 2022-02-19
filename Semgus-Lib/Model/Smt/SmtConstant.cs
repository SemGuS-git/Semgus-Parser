using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    public class SmtConstant : SmtFunction
    {
        public SmtConstant(SmtIdentifier name, SmtSort sort) : base(name, SmtTheory.UserDefined, new SmtFunctionRank(sort)) { }
    }
}
