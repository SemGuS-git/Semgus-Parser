using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Theories
{
    public class SmtIntsTheory : ISmtTheory
    {
        public static SmtIntsTheory Instance { get; } = new(SmtCoreTheory.Instance);

        private class IntSort : SmtSort
        {
            private IntSort() : base(SmtCommonIdentifiers.SORT_INT) { }
            public static IntSort Instance { get; } = new();
        }

        public IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions { get; }
        public IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts { get; }

        private SmtIntsTheory(SmtCoreTheory core)
        {
            SmtSort i = IntSort.Instance;
            SmtSort b = core.Sorts[SmtCommonIdentifiers.SORT_BOOL.Name];

            Dictionary<SmtIdentifier, SmtFunction> fd = new();
            void cf(string name, SmtSort ret, params SmtSort[] args)
            {
                SmtIdentifier id = new(name);
                if (fd.TryGetValue(id, out SmtFunction? fun))
                {
                    fun.AddRankTemplate(new SmtFunctionRank(ret, args));
                }
                else
                {
                    fd.Add(id, new SmtFunction(id, this, new SmtFunctionRank(ret, args)));
                }
            }

            Sorts = new Dictionary<SmtIdentifier, SmtSort>() { { i.Name.Name, i } };

            cf("-", i, i); // Negation
            cf("-", i, i, i); // Subtraction
            cf("+", i, i, i);
            cf("*", i, i, i);
            cf("div", i, i, i);
            cf("mod", i, i, i);
            cf("abs", i, i);
            cf("<=", b, i, i);
            cf("<", b, i, i);
            cf(">=", b, i, i);
            cf(">", b, i, i);

            Functions = fd;
        }

    }
}
