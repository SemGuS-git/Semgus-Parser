using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model;
using Semgus.Model.Smt;

#nullable enable

namespace Semgus.Parser
{
    /// <summary>
    /// Interface that receives messages during reading of a SemGuS problem file
    /// </summary>
    public interface ISemgusProblemHandler
    {
        public void OnTermTypes(IReadOnlyList<TermType> termTypes);

        public void OnSynthFun(SmtContext ctx, SmtIdentifier name, IList<SmtConstant> args, SmtSort sort);

        public void AddLibraryFunction(SmtFunction fun);

        public void OnSetInfo(SmtContext ctx, SmtKeyword keyword);
    }
}
