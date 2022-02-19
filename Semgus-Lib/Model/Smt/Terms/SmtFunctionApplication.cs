using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    public class SmtFunctionApplication : SmtTerm
    {
        public SmtFunction Definition { get; private set; }
        public SmtFunctionRank Rank { get; private set; }
        public IReadOnlyList<SmtTerm> Arguments { get; private set; }

        public SmtFunctionApplication(SmtFunction defn, SmtFunctionRank rank, IList<SmtTerm> arguments) : base(rank.ReturnSort)
        {
            Definition = defn;
            Rank = rank;
            Arguments = arguments.ToList();
        }

        public override string ToString()
        {
            if (Arguments.Count == 0)
            {
                return Definition.Name.ToString();
            }
            else
            {
                return $"({Definition.Name} {string.Join(' ', Arguments.Select(a => a.ToString()))})";
            }
        }

        public override TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor)
        {
            return visitor.VisitFunctionApplication(this);
        }
    }
}
