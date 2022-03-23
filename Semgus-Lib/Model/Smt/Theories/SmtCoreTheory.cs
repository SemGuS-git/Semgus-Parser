using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Theories
{
    using static SmtCommonIdentifiers;

    public class SmtCoreTheory : ISmtTheory {
        public static SmtCoreTheory Instance { get; } = new();

        private class BoolSort : SmtSort {
            private BoolSort() : base(BoolSortId) { }
            public static BoolSort Instance { get; } = new();
        }
        public IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions { get; }
        public IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts { get; }

        private SmtCoreTheory()
        {
            SmtSort b = BoolSort.Instance;
            SmtSort.UniqueSortFactory usf = new();

            Dictionary<SmtIdentifier, SmtFunction> fd = new();
            void cf(SmtIdentifier id, SmtSort ret, params SmtSort[] args)
            {
                if (fd.TryGetValue(id, out SmtFunction? fun))
                {
                    fun.AddRankTemplate(new SmtFunctionRank(ret, args));
                }
                else
                {
                    fd.Add(id, new SmtFunction(id, this, new SmtFunctionRank(ret, args)));
                }
            }

            Sorts = new Dictionary<SmtIdentifier, SmtSort>() { { b.Name, b } };

            var id_and = AndFunctionId;
            var id_or = OrFunctionId;
            var id_eq = EqFunctionId;

            cf(new("true"), b);
            cf(new("false"), b);
            cf(new("not"), b, b);

            cf(id_and, b, b, b);
            cf(id_and, b, b, b, b);
            cf(id_and, b, b, b, b, b);
            cf(id_and, b, b, b, b, b, b);
            cf(id_and, b, b, b, b, b, b, b);
            cf(id_and, b, b, b, b, b, b, b, b);

            cf(id_or, b, b, b);
            cf(id_or, b, b, b, b);
            cf(id_or, b, b, b, b, b);
            cf(id_or, b, b, b, b, b, b);
            cf(id_or, b, b, b, b, b, b, b);
            cf(id_or, b, b, b, b, b, b, b, b);

            cf(new("!"), b, b);
            cf(new("xor"), b, b, b);
            cf(new("=>"), b, b, b);
            cf(id_eq, b, usf.Sort, usf.Sort);
            cf(new("distinct"), b, usf.Next(), usf.Sort);
            cf(new("ite"), usf.Next(), b, usf.Sort, usf.Sort);

            Functions = fd;
        }

    }
}
