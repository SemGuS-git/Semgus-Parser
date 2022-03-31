using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    public class SmtConstant : SmtFunction
    {
        public SmtConstant(SmtIdentifier name, ISmtTheory theory, SmtSort sort) : base(name, theory, new SmtFunctionRank(sort)) { }
    }
}
