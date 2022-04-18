using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Theories
{
    using static SmtCommonIdentifiers;

    internal class SmtIntsTheory : ISmtTheory
    {
        public static SmtIntsTheory Instance { get; } = new(SmtCoreTheory.Instance);

        private class IntSort : SmtSort
        {
            private IntSort() : base(IntSortId) { }
            public static IntSort Instance { get; } = new();
        }

        public SmtIdentifier Name { get; } = IntsTheoryId;
        public IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions { get; }
        public IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts { get; }
        public IReadOnlySet<SmtIdentifier> PrimarySortSymbols { get; }
        public IReadOnlySet<SmtIdentifier> PrimaryFunctionSymbols { get; }

        /// <summary>
        /// Gets the requested sort (for the Int sort)
        /// </summary>
        /// <param name="ssi">Sort identifier</param>
        /// <param name="sort">The Int sort</param>
        /// <returns>True if requesting the Int sort, false if not</returns>
        public bool TryGetSort(SmtSortIdentifier ssi, [NotNullWhen(true)] out SmtSort? sort)
        {
            if (ssi == IntSortId)
            {
                sort = IntSort.Instance;
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

        private SmtIntsTheory(SmtCoreTheory core)
        {
            SmtSort i = IntSort.Instance;
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

            Sorts = new Dictionary<SmtIdentifier, SmtSort>() { { i.Name.Name, i } };
            PrimarySortSymbols = new HashSet<SmtIdentifier>() { i.Name.Name };

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
            PrimaryFunctionSymbols = new HashSet<SmtIdentifier>(fd.Keys);
        }
    }
}
