using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Theories
{
    internal class SmtIntsTheory : SmtTheory
    {
        private class IntSort : SmtSort
        {
            private IntSort() : base(new SmtIdentifier("Int")) { }
            public static IntSort Instance { get; } = new();
        }

        private readonly IReadOnlyDictionary<SmtIdentifier, SmtSort> _intSortList;

        private readonly IReadOnlyDictionary<SmtIdentifier, SmtFunction> _functions;

        public SmtIntsTheory(SmtCoreTheory core)
        {
            SmtSort i = IntSort.Instance;
            SmtSort b = core.Sorts[new SmtIdentifier("Bool")];

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

            _intSortList = new Dictionary<SmtIdentifier, SmtSort>() { { i.Name, i } };

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

            _functions = fd;
        }

        public override IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions => _functions;

        public override IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts => _intSortList;
    }
}
