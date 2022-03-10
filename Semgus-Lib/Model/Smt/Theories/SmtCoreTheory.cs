using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Theories
{
    internal class SmtCoreTheory : ISmtTheory {
        public static SmtCoreTheory Instance { get; } = new();

        private class BoolSort : SmtSort {
            private BoolSort() : base(new SmtIdentifier("Bool")) { }
            public static BoolSort Instance { get; } = new();
        }
        public IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions { get; }
        public IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts { get; }

        private SmtCoreTheory()
        {
            SmtSort b = BoolSort.Instance;
            SmtSort.UniqueSortFactory usf = new();

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
                    fd.Add(id, new SmtFunction(id, new SmtFunctionRank(ret, args)));
                }
            }

            Sorts = new Dictionary<SmtIdentifier, SmtSort>() { { b.Name, b } };

            cf("true", b);
            cf("false", b);
            cf("not", b, b);

            cf("and", b, b, b);
            cf("and", b, b, b, b);
            cf("and", b, b, b, b, b);
            cf("and", b, b, b, b, b, b);
            cf("and", b, b, b, b, b, b, b);
            cf("and", b, b, b, b, b, b, b, b);

            cf("or", b, b, b);
            cf("or", b, b, b, b);
            cf("or", b, b, b, b, b);
            cf("or", b, b, b, b, b, b);
            cf("or", b, b, b, b, b, b, b);
            cf("or", b, b, b, b, b, b, b, b);

            cf("xor", b, b, b);
            cf("=>", b, b, b);
            cf("=", b, usf.Sort, usf.Sort);
            cf("distinct", b, usf.Next(), usf.Sort);
            cf("ite", usf.Next(), b, usf.Sort, usf.Sort);

            Functions = fd;
        }

    }
}
