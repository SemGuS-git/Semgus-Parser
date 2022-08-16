
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
        public IReadOnlyDictionary<SmtIdentifier, IApplicable> Functions { get; }
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
        public bool TryGetFunction(SmtIdentifier fid, [NotNullWhen(true)] out IApplicable? function)
            => Functions.TryGetValue(fid, out function);

        private SmtIntsTheory(SmtCoreTheory core)
        {
            SmtSort i = IntSort.Instance;
            SmtSort b = core.Sorts[BoolSortId.Name];

            SmtSourceBuilder sb = new(this);
            sb.AddSort(i);

            sb.AddFn("-", i, i); // Negation
            sb.AddFn("-", i, i, i); // Subtraction
            sb.AddFn("+", i, i, i);
            sb.AddFn("*", i, i, i);
            sb.AddFn("div", i, i, i);
            sb.AddFn("mod", i, i, i);
            sb.AddFn("abs", i, i);
            sb.AddFn("<=", b, i, i);
            sb.AddFn("<", b, i, i);
            sb.AddFn(">=", b, i, i);
            sb.AddFn(">", b, i, i);

            Functions = sb.Functions;
            PrimaryFunctionSymbols = sb.PrimaryFunctionSymbols;
            Sorts = sb.Sorts;
            PrimarySortSymbols = sb.PrimarySortSymbols;
        }
    }
}
