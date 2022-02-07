using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// A rank is a particular non-empty sequence of sorts for a function symbol
    /// </summary>
    public class SmtFunctionRank
    {
        public SmtFunctionRank(SmtSort returnSort, params SmtSort[] argumentSorts)
        {
            ReturnSort = returnSort;
            ArgumentSorts = argumentSorts;
        }

        public SmtSort ReturnSort { get; private set; }
        public IReadOnlyList<SmtSort> ArgumentSorts { get; private set; }
        public int Arity => ArgumentSorts.Count;
        public bool IsParametric => ReturnSort.IsParametric || ArgumentSorts.Any(s => s.IsParametric);
    }
}
