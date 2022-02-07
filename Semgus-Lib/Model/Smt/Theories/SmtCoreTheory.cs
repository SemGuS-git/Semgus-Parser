using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Theories
{
    internal class SmtCoreTheory : SmtTheory
    {
        private class BoolSort : SmtSort
        {
            private BoolSort() : base(new SmtIdentifier("Bool")) { }
            public static BoolSort Instance { get; } = new();
        }

        private readonly IReadOnlyDictionary<SmtIdentifier, SmtSort> _boolSortList;

        private readonly IReadOnlyDictionary<SmtIdentifier, SmtFunction> _functions;

        public SmtCoreTheory()
        {
            SmtSort b = BoolSort.Instance;
            SmtSort.UniqueSortFactory usf = new();

            Dictionary<SmtIdentifier, SmtFunction> fd = new();
            void cf(string name, SmtSort ret, params SmtSort[] args)
            {
                SmtIdentifier id = new(name);
                fd.Add(id, new SmtFunction(id, this, new SmtFunctionRank(ret, args)));
            }

            _boolSortList = new Dictionary<SmtIdentifier, SmtSort>() { { b.Name, b } };

            cf("true", b);
            cf("false", b);
            cf("not", b, b);
            cf("and", b, b, b);
            cf("or", b, b, b);
            cf("xor", b, b, b);
            cf("=>", b, b, b);
            cf("=", b, usf.Sort, usf.Sort);
            cf("distinct", b, usf.Next(), usf.Sort);
            cf("ite", usf.Next(), b, usf.Sort, usf.Sort);

            _functions = fd;
        }

        public override IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions => _functions;

        public override IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts => _boolSortList;
    }
}
