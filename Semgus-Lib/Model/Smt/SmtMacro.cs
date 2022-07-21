using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// Function-like object that can be expanded into SMT expressions
    /// </summary>
    public class SmtMacro : IApplicable
    {
        public SmtIdentifier Name => throw new NotImplementedException();

        public ISmtTheory Theory => throw new NotImplementedException();

        public string GetRankHelp()
        {
            throw new NotImplementedException();
        }

        public bool IsArityPossible(int arity)
        {
            throw new NotImplementedException();
        }

        public bool TryResolveRank([NotNullWhen(true)] out SmtFunctionRank? rank, SmtSort? returnSort, params SmtSort[] argumentSorts)
        {
            throw new NotImplementedException();
        }
    }
}
