using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Theories
{
    using static SmtCommonIdentifiers;

    internal class SmtStringsTheory : ISmtTheory
    {
        public static SmtStringsTheory Instance { get; } = new(SmtCoreTheory.Instance, SmtIntsTheory.Instance);

        private class StringSort : SmtSort
        {
            private StringSort() : base(StringSortId) { }
            public static StringSort Instance { get; } = new();
        }
        public SmtIdentifier Name { get; } = StringsTheoryId;
        public IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions { get; }
        public IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts { get; }
        public IReadOnlySet<SmtIdentifier> PrimarySortSymbols { get; }
        public IReadOnlySet<SmtIdentifier> PrimaryFunctionSymbols { get; }

        /// <summary>
        /// Gets the requested sort (for the String sort)
        /// </summary>
        /// <param name="ssi">Sort identifier</param>
        /// <param name="sort">The String sort</param>
        /// <returns>True if requesting the String sort, false if not</returns>
        public bool TryGetSort(SmtSortIdentifier ssi, [NotNullWhen(true)] out SmtSort? sort)
        {
            if (ssi == StringSortId)
            {
                sort = StringSort.Instance;
                return true;
            }
            else
            {
                sort = default;
                return false;
            }
        }

        /// <summary>
        /// Gets the requested function
        /// </summary>
        /// <param name="fid">Function identifier</param>
        /// <param name="function">The requested function</param>
        /// <returns>True if successfully got function, false otherwise</returns>
        public bool TryGetFunction(SmtIdentifier fid, [NotNullWhen(true)] out SmtFunction? function)
            => Functions.TryGetValue(fid, out function);

        private SmtStringsTheory(SmtCoreTheory core, SmtIntsTheory ints)
        {
            SmtSort s = StringSort.Instance;
            SmtSort i = ints.Sorts[IntSortId.Name];
            SmtSort b = core.Sorts[BoolSortId.Name];

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
            
            Sorts = new Dictionary<SmtIdentifier, SmtSort>() { { s.Name.Name, s } };
            PrimarySortSymbols = new HashSet<SmtIdentifier>() { s.Name.Name };

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
            PrimaryFunctionSymbols = new HashSet<SmtIdentifier>(fd.Keys);
        }
    }
}
