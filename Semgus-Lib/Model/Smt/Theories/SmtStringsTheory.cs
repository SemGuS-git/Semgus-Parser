
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

        /// <summary>
        /// Sort of strings
        /// </summary>
        private class StringSort : SmtSort
        {
            private StringSort() : base(StringSortId) { }
            public static StringSort Instance { get; } = new();
        }

        /// <summary>
        /// Sort of regular languages, i.e., regular expressions
        /// </summary>
        private class RegLanSort : SmtSort
        {
            private RegLanSort() : base(RegLanSortId) { }
            public static RegLanSort Instance { get; } = new();
        }

        public SmtIdentifier Name { get; } = StringsTheoryId;
        public IReadOnlyDictionary<SmtIdentifier, IApplicable> Functions { get; }
        public IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts { get; }
        public IReadOnlySet<SmtIdentifier> PrimarySortSymbols { get; }
        public IReadOnlySet<SmtIdentifier> PrimaryFunctionSymbols { get; }

        /// <summary>
        /// Gets the requested sort (for the String or RegLan sorts)
        /// </summary>
        /// <param name="ssi">Sort identifier</param>
        /// <param name="sort">The String or RegLan sort</param>
        /// <returns>True if requesting the String or RegLan sort, false if not</returns>
        public bool TryGetSort(SmtSortIdentifier ssi, [NotNullWhen(true)] out SmtSort? sort)
        {
            if (ssi == StringSortId)
            {
                sort = StringSort.Instance;
                return true;
            }
            else if (ssi == RegLanSortId)
            {
                sort = RegLanSort.Instance;
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
        public bool TryGetFunction(SmtIdentifier fid, [NotNullWhen(true)] out IApplicable? function)
            => Functions.TryGetValue(fid, out function);

        private SmtStringsTheory(SmtCoreTheory core, SmtIntsTheory ints)
        {
            SmtSort s = StringSort.Instance;
            SmtSort r = RegLanSort.Instance;
            SmtSort i = ints.Sorts[IntSortId.Name];
            SmtSort b = core.Sorts[BoolSortId.Name];

            SmtSourceBuilder sb = new(this);
            sb.AddSort(s);
            sb.AddSort(r);

            // TODO: the rest of the regular expression functions
            sb.AddFn("str.++", s, s, s);
            sb.AddFn("str.len", i, s);
            sb.AddFn("str.<", b, s, s);
            sb.AddFn("str.<=", b, s, s);
            sb.AddFn("str.at", s, s, i);
            sb.AddFn("str.substr", s, s, i, i);
            sb.AddFn("str.prefixof", b, s, s);
            sb.AddFn("str.suffixof", b, s, s);
            sb.AddFn("str.contains", b, s, s);
            sb.AddFn("str.indexof", i, s, s, i);
            sb.AddFn("str.replace", s, s, s, s);
            sb.AddFn("str.replace_all", s, s, s, s);
            sb.AddFn("str.is_digit", b, s);
            sb.AddFn("str.to_code", i, s);
            sb.AddFn("str.from_code", s, i);
            sb.AddFn("str.to_int", i, s);
            sb.AddFn("str.from_int", s, i);

            sb.AddFn("str.to_re", r, s);
            sb.AddFn("str.in_re", b, s, r);
            sb.AddFn("re.none", r);
            sb.AddFn("re.all", r);
            sb.AddFn("re.allchar", r);
            sb.AddFn("re.++", r, r, r);
            sb.AddFn("re.union", r, r, r);
            sb.AddFn("re.inter", r, r, r);
            sb.AddFn("re.*", r, r);
            sb.AddFn("re.comp", r, r);
            sb.AddFn("re.diff", r, r, r);
            sb.AddFn("re.+", r, r);
            sb.AddFn("re.opt", r, r);
            sb.AddFn("re.range", r, s, s);

            Functions = sb.Functions;
            PrimaryFunctionSymbols = sb.PrimaryFunctionSymbols;
            Sorts = sb.Sorts;
            PrimarySortSymbols = sb.PrimarySortSymbols;
        }
    }
}
