using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Theories
{
    internal class SmtStringsTheory : SmtTheory
    {
        private class StringSort : SmtSort
        {
            private StringSort() : base(new SmtIdentifier("String")) { }
            public static StringSort Instance { get; } = new();
        }

        private readonly IReadOnlyDictionary<SmtIdentifier, SmtSort> _stringSortList;

        private readonly IReadOnlyDictionary<SmtIdentifier, SmtFunction> _functions;

        public SmtStringsTheory(SmtCoreTheory core, SmtIntsTheory ints)
        {
            SmtSort s = StringSort.Instance;
            SmtSort i = ints.Sorts[new SmtIdentifier("Int")];
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

            _stringSortList = new Dictionary<SmtIdentifier, SmtSort>() { { s.Name, s } };
            /*
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
            */
            _functions = fd;
        }

        public override IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions => _functions;

        public override IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts => _stringSortList;
    }
}
