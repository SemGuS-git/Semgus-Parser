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
    /// The theory of arrays with extensions
    /// </summary>
    internal class SmtArraysExTheory : ISmtTheory
    {
        /// <summary>
        /// A singleton theory instance
        /// </summary>
        public static SmtArraysExTheory Instance { get; } = new();

        /// <summary>
        /// Underlying array sort
        /// </summary>
        internal sealed class ArraySort : SmtSort
        {
            /// <summary>
            /// Cache of instantiated sorts. We need this since sorts are compared by reference
            /// </summary>
            private static readonly IDictionary<(SmtSortIdentifier, SmtSortIdentifier), ArraySort> _sortCache
                = new Dictionary<(SmtSortIdentifier, SmtSortIdentifier), ArraySort>();

            /// <summary>
            /// The sort used for indexing the array
            /// </summary>
            public SmtSort IndexSort { get; private set; }

            /// <summary>
            /// The sort used for the array element values
            /// </summary>
            public SmtSort ValueSort { get; private set; }

            /// <summary>
            /// Constructs a new array sort with the given parameters
            /// </summary>
            /// <param name="size">Size of bit vectors in this sort</param>
            private ArraySort(SmtSortIdentifier indexSort, SmtSortIdentifier valueSort) :
                base(new(new SmtIdentifier(ArraySortPrimaryId.Symbol), indexSort, valueSort))
            {
                IndexSort = new UnresolvedParameterSort(indexSort);
                ValueSort = new UnresolvedParameterSort(valueSort);
                IsParametric = true;
                Arity = 2;
            }

            /// <summary>
            /// Gets the array sort for the given index and value sorts
            /// </summary>
            /// <param name="index">The index sort to use</param>
            /// <param name="value">The value sort to use</param>
            /// <returns>The array sort for the given index and value sorts</returns>
            public static ArraySort GetSort(SmtSortIdentifier index, SmtSortIdentifier value)
            {
                if (_sortCache.TryGetValue((index, value), out ArraySort? sort))
                {
                    return sort;
                }
                else
                {
                    sort = new ArraySort(index, value);
                    _sortCache.Add((index, value), sort);
                    return sort;
                }
            }

            /// <summary>
            /// Updates this sort with the resolved parameteric sorts
            /// </summary>
            /// <param name="resolved">Resolved parameters. Must have arity 2</param>
            public override void UpdateForResolvedParameters(IList<SmtSort> resolved)
            {
                if (resolved.Count != 2)
                {
                    throw new InvalidOperationException("Got list of resolved sorts not of length 2!");
                }

                IndexSort = resolved[0];
                ValueSort = resolved[1];
            }
        }

        /// <summary>
        /// This theory's name
        /// </summary>
        public SmtIdentifier Name { get; } = ArraysExTheoryId;

        #region Deprecated
        public IReadOnlyDictionary<SmtIdentifier, IApplicable> Functions { get; }
        #endregion

        /// <summary>
        /// The primary (i.e., non-indexed) sort symbols (e.g., "Array")
        /// </summary>
        public IReadOnlySet<SmtIdentifier> PrimarySortSymbols { get; }

        /// <summary>
        /// The primary (i.e., non-indexed) function symbols
        /// </summary>
        public IReadOnlySet<SmtIdentifier> PrimaryFunctionSymbols { get; }

        /// <summary>
        /// Constructs an instance of the theory of arrays
        /// </summary>
        /// <param name="core">Reference to the core theory</param>
        private SmtArraysExTheory()
        {
            SmtSourceBuilder sb = new(this);
            sb.AddOnTheFlyFn("select");
            sb.AddOnTheFlyFn("store");

            Functions = sb.Functions;
            PrimaryFunctionSymbols = sb.PrimaryFunctionSymbols;
            PrimarySortSymbols = new HashSet<SmtIdentifier>() { ArraySortPrimaryId };
        }

        /// <summary>
        /// Looks up a sort symbol in this theory
        /// </summary>
        /// <param name="sortId">The sort ID</param>
        /// <param name="resolvedSort">The resolved sort</param>
        /// <returns>True if successfully gotten</returns>
        public bool TryGetSort(SmtSortIdentifier sortId, [NotNullWhen(true)] out SmtSort? resolvedSort)
        {
            if (sortId.Arity == 2 && sortId.Name == ArraySortPrimaryId)
            {
                resolvedSort = ArraySort.GetSort(sortId.Parameters[0], sortId.Parameters[1]);
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
