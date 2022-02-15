using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

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

        public void OnSetInfo(SmtContext ctx, SmtKeyword keyword);

        public void OnConstraint(SmtContext smtCtx, SemgusContext semgusCxt, SmtTerm constraint);

        public void OnCheckSynth(SmtContext smtCtx, SemgusContext semgusCtx);
    }
}
