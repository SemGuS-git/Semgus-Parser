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

            Dictionary<SmtIdentifier, SmtFunction> fd = new();
            void cf(SmtIdentifier id, SmtSort ret, params SmtSort[] args)
            {
                if (fd.TryGetValue(id, out SmtFunction? fun))
                {
                    fun.AddRankTemplate(new SmtFunctionRank(ret, args));
                }
                else
                {
                    fd.Add(id, new SmtFunction(id, this, new SmtFunctionRank(ret, args)));
                }
            }

            Sorts = new Dictionary<SmtIdentifier, SmtSort>() { { b.Name.Name, b } };
            PrimarySortSymbols = new HashSet<SmtIdentifier>() { b.Name.Name };

            var id_and = AndFunctionId;
            var id_or = OrFunctionId;
            var id_eq = EqFunctionId;

            cf(new("true"), b);
            cf(new("false"), b);
            cf(new("not"), b, b);

            cf(id_and, b, b, b);

            cf(id_or, b, b, b);

            cf(new("!"), b, b);
            cf(new("xor"), b, b, b);
            cf(new("=>"), b, b, b);
            cf(id_eq, b, usf.Sort, usf.Sort);
            cf(new("distinct"), b, usf.Next(), usf.Sort);
            cf(new("ite"), usf.Next(), b, usf.Sort, usf.Sort);

            Dictionary<SmtIdentifier, SmtMacro> md = new();
            void cm(SmtIdentifier id, SmtMacro.DefaultMacroType defType)
            {
                md.Add(id, new SmtMacro(fd[id], defType, expandByDefault: false));
            }

            cm(id_and, SmtMacro.DefaultMacroType.LeftAssociative);
            cm(id_or, SmtMacro.DefaultMacroType.LeftAssociative);

            Functions = fd.ToDictionary(kvp => kvp.Key, kvp => (IApplicable)kvp.Value);
            PrimaryFunctionSymbols = new HashSet<SmtIdentifier>(fd.Keys);
            Macros = md;
        }
    }
}
