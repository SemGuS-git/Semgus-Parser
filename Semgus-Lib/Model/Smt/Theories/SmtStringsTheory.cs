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
            private StringSort() : base(new SmtSortIdentifier("String")) { }
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

            _stringSortList = new Dictionary<SmtIdentifier, SmtSort>() { { s.Name.Name, s } };

            // TODO: regular expression functions
            cf("str.++", s, s, s);
            cf("str.len", i, s);
            cf("str.<", b, s, s);
            cf("str.<=", b, s, s);
            cf("str.at", s, s, i);
            cf("str.substr", s, s, i, i);
            cf("str.prefixof", b, s, s);
            cf("str.suffixof", b, s, s);
            cf("str.contains", b, s, s);
            cf("str.indexof", i, s, s, i);
            cf("str.replace", s, s, s, s);
            cf("str.replace_all", s, s, s, s);
            cf("str.is_digit", b, s);
            cf("str.to_code", i, s);
            cf("str.from_code", s, i);
            cf("str.to_int", i, s);
            cf("str.from_int", s, i);

            _functions = fd;
        }

        public override IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions => _functions;

        public override IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts => _stringSortList;
    }
}
