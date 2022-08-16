
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Theories
{
    using static SmtCommonIdentifiers;

    internal class SmtCoreTheory : ISmtTheory
    {
        public static SmtCoreTheory Instance { get; } = new();

        private class BoolSort : SmtSort {
            private BoolSort() : base(BoolSortId) { }
            public static BoolSort Instance { get; } = new();
        }
        public SmtIdentifier Name { get; } = CoreTheoryId;
        public IReadOnlyDictionary<SmtIdentifier, IApplicable> Functions { get; }
        public IReadOnlyDictionary<SmtIdentifier, SmtMacro> Macros { get; }
        public IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts { get; }
        public IReadOnlySet<SmtIdentifier> PrimarySortSymbols { get; }
        public IReadOnlySet<SmtIdentifier> PrimaryFunctionSymbols { get; }

        /// <summary>
        /// Gets the requested sort (for the Bool sort)
        /// </summary>
        /// <param name="ssi">Sort identifier</param>
        /// <param name="sort">The Bool sort</param>
        /// <returns>True if requesting the Bool sort, false if not</returns>
        public bool TryGetSort(SmtSortIdentifier ssi, [NotNullWhen(true)] out SmtSort? sort)
        {
            if (ssi == BoolSortId)
            {
                sort = BoolSort.Instance;
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
        {
            if (Macros.TryGetValue(fid, out SmtMacro? macro))
            {
                function = macro;
                return true;
            }
            return Functions.TryGetValue(fid, out function);
        }

        private SmtCoreTheory()
        {
            SmtSort b = BoolSort.Instance;
            SmtSort.UniqueSortFactory usf = new();

            SmtSourceBuilder sb = new(this);
            sb.AddSort(b);

            var id_and = AndFunctionId;
            var id_or = OrFunctionId;
            var id_eq = EqFunctionId;

            sb.AddFn("true", b);
            sb.AddFn("false", b);
            sb.AddFn("not", b, b);

            sb.AddFn(id_and, b, b, b);

            sb.AddFn(id_or, b, b, b);

            sb.AddFn("!", b, b);
            sb.AddFn("xor", b, b, b);
            sb.AddFn("=>", b, b, b);
            sb.AddFn(id_eq, b, usf.Sort, usf.Sort);
            sb.AddFn("distinct", b, usf.Next(), usf.Sort);
            sb.AddFn("ite", usf.Next(), b, usf.Sort, usf.Sort);


            sb.AddMacro(id_and, SmtMacro.DefaultMacroType.LeftAssociative);
            sb.AddMacro(id_or, SmtMacro.DefaultMacroType.LeftAssociative);

            Functions = sb.Functions;
            PrimaryFunctionSymbols = sb.PrimaryFunctionSymbols;
            Macros = sb.Macros;
            Sorts = sb.Sorts;
            PrimarySortSymbols = sb.PrimarySortSymbols;
        }
    }
}
