using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Theories
{
    public class SmtStringsTheory : ISmtTheory
    {
        public static SmtStringsTheory Instance { get; } = new(SmtCoreTheory.Instance, SmtIntsTheory.Instance);

        private class StringSort : SmtSort
        {
            private StringSort() : base(SmtCommonIdentifiers.SORT_STRING) { }
            public static StringSort Instance { get; } = new();
        }

        public IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions { get; }
        public IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts { get; }

        private SmtStringsTheory(SmtCoreTheory core, SmtIntsTheory ints)
        {
            SmtSort s = StringSort.Instance;
            SmtSort i = ints.Sorts[SmtCommonIdentifiers.SORT_INT.Name];
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
                    fd.Add(id, new SmtFunction(id, new SmtFunctionRank(ret, args)));
                }
            }
            
            Sorts = new Dictionary<SmtIdentifier, SmtSort>() { { s.Name.Name, s } };

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

            Functions = fd;
        }
    }
}
