using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Semgus.Model.Smt.SmtCommonIdentifiers;

namespace Semgus.Model.Smt.Theories
{
    /// <summary>
    /// The (non-standard) theory of sequences
    /// https://cvc5.github.io/docs-ci/docs-main/theories/sequences.html
    /// </summary>
    internal class SmtSequencesTheory : ISmtTheory
    {
        /// <summary>
        /// A singleton theory instance
        /// </summary>
        public static SmtSequencesTheory Instance { get; } = new(SmtCoreTheory.Instance, SmtIntsTheory.Instance);

        /// <summary>
        /// Underlying sequence sort
        /// </summary>
        internal sealed class SeqSort : SmtSort
        {
            /// <summary>
            /// Cache of instantiated sorts. We need this since sorts are compared by reference
            /// </summary>
            private static readonly IDictionary<SmtSortIdentifier, SeqSort> _sortCache
                = new Dictionary<SmtSortIdentifier, SeqSort>();

            /// <summary>
            /// The sort used for the sequence element values
            /// </summary>
            public SmtSort ElementSort { get; private set; }

            /// <summary>
            /// This sort's parameters
            /// </summary>
            public override IEnumerable<SmtSort> Parameters
            {
                get => new SmtSort[] { ElementSort };
            }

            /// <summary>
            /// Constructs a new seq sort with the given element type
            /// </summary>
            /// <param name="elementSort">Sort for sequence elements</param>
            private SeqSort(SmtSortIdentifier elementSort) :
                base(new(new SmtIdentifier(SeqSortPrimaryId.Symbol), elementSort))
            {
                ElementSort = new UnresolvedParameterSort(elementSort);
                IsParametric = true;
                Arity = 1;
            }

            /// <summary>
            /// Constructs a new seq sort with the given (possibly concrete) element type
            /// </summary>
            /// <param name="elementSort">Sort for sequence elements</param>
            internal SeqSort(SmtSort elementSort)
                : base(new(SeqSortPrimaryId, elementSort.Name))
            {
                ElementSort = elementSort;
                IsSortParameter = elementSort.IsSortParameter;
                Arity = 1;
            }

            /// <summary>
            /// Gets the seq sort for the given element sort
            /// </summary>
            /// <param name="element">The id of the element sort</param>
            /// <returns>The seq sort for the given element sort</returns>
            public static SeqSort GetSort(SmtSortIdentifier element)
            {
                if (_sortCache.TryGetValue(element, out SeqSort? sort))
                {
                    return sort;
                }
                else
                {
                    sort = new SeqSort(element);
                    _sortCache.Add(element, sort);
                    return sort;
                }
            }

            /// <summary>
            /// Updates this sort with the resolved parameteric sorts
            /// </summary>
            /// <param name="resolved">Resolved parameters. Must have arity 1</param>
            public override void UpdateForResolvedParameters(IList<SmtSort> resolved)
            {
                if (resolved.Count != 1)
                {
                    throw new InvalidOperationException("Got list of resolved sorts not of length 1!");
                }

                ElementSort = resolved[0];
            }
        }

        /// <summary>
        /// This theory's name
        /// </summary>
        public SmtIdentifier Name { get; } = SequencesTheoryId;

        #region Deprecated
        public IReadOnlyDictionary<SmtIdentifier, IApplicable> Functions { get; }
        #endregion

        /// <summary>
        /// The primary (i.e., non-indexed) sort symbols (e.g., "Seq")
        /// </summary>
        public IReadOnlySet<SmtIdentifier> PrimarySortSymbols { get; }

        /// <summary>
        /// The primary (i.e., non-indexed) function symbols
        /// </summary>
        public IReadOnlySet<SmtIdentifier> PrimaryFunctionSymbols { get; }

        /// <summary>
        /// Constructs an instance of the theory of sequences
        /// </summary>
        /// <param name="core">Reference to the core theory</param>
        private SmtSequencesTheory(SmtCoreTheory core, SmtIntsTheory ints)
        {
            SmtSort b = core.Sorts[BoolSortId.Name];
            SmtSort i = ints.Sorts[IntSortId.Name];

            SmtSourceBuilder sb = new(this);
            var usf = new SmtSort.UniqueSortFactory();
            var elementSort = usf.Next();
            var seqSort = new SeqSort(elementSort);
            sb.AddFn("seq.empty", seqSort);
            sb.AddFn("seq.unit", seqSort, elementSort);
            sb.AddFn("seq.len", i, seqSort);
            sb.AddFn("seq.nth", elementSort, seqSort, i);
            sb.AddFn("seq.update", seqSort, seqSort, i, seqSort);
            sb.AddFn("seq.extract", seqSort, seqSort, i, i);
            sb.AddFn("seq.++", seqSort, seqSort, seqSort);
            sb.AddFn("seq.at", seqSort, seqSort, i);
            sb.AddFn("seq.contains", b, seqSort, seqSort);
            sb.AddFn("seq.indexof", i, seqSort, seqSort, i);
            sb.AddFn("seq.replace", seqSort, seqSort, seqSort, seqSort);
            sb.AddFn("seq.replace_all", seqSort, seqSort, seqSort, seqSort);
            sb.AddFn("seq.rev", seqSort, seqSort);
            sb.AddFn("seq.prefixof", b, seqSort, seqSort);
            sb.AddFn("seq.suffixof", b, seqSort, seqSort);

            Functions = sb.Functions;
            PrimaryFunctionSymbols = sb.PrimaryFunctionSymbols;
            PrimarySortSymbols = new HashSet<SmtIdentifier>() { SeqSortPrimaryId };
        }

        /// <summary>
        /// Looks up a sort symbol in this theory
        /// </summary>
        /// <param name="sortId">The sort ID</param>
        /// <param name="resolvedSort">The resolved sort</param>
        /// <returns>True if successfully gotten</returns>
        public bool TryGetSort(SmtSortIdentifier sortId, [NotNullWhen(true)] out SmtSort? resolvedSort)
        {
            if (sortId.Arity == 1 && sortId.Name == SeqSortPrimaryId)
            {
                resolvedSort = SeqSort.GetSort(sortId.Parameters[0]);
                return true;
            }
            resolvedSort = default;
            return false;
        }

        /// <summary>
        /// Looks up a function in this theory
        /// </summary>
        /// <param name="functionId">The function ID to look up</param>
        /// <param name="resolvedFunction">The resolved function</param>
        /// <returns>True if successfully gotten</returns>
        public bool TryGetFunction(SmtIdentifier functionId, [NotNullWhen(true)] out IApplicable? resolvedFunction)
        {
            return Functions.TryGetValue(functionId, out resolvedFunction);
        }
    }
}
