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
    /// The theory of fixed-size bit vectors
    /// </summary>
    internal class SmtBitVectorsTheory : ISmtTheory
    {
        /// <summary>
        /// A singleton theory instance
        /// </summary>
        public static SmtBitVectorsTheory Instance { get; } = new(SmtCoreTheory.Instance);

        /// <summary>
        /// Underlying bit vector sort
        /// </summary>
        internal sealed class BitVectorsSort : SmtSort
        {
            /// <summary>
            /// Cache of instantiated sorts. We need this since sorts are compared by reference
            /// </summary>
            private static readonly IDictionary<long, BitVectorsSort> _sortCache = new Dictionary<long, BitVectorsSort>();

            /// <summary>
            /// Size of bit vectors in this sort. Must be greater than 0
            /// </summary>
            public long Size { get; }

            /// <summary>
            /// Constructs a new bit vector sort of the given size
            /// </summary>
            /// <param name="size">Size of bit vectors in this sort</param>
            private BitVectorsSort(long size) : base(new(new SmtIdentifier(BitVectorSortPrimaryId.Symbol,
                                                                      size)))
            {
                Size = size;
            }

            /// <summary>
            /// Gets the bit vector sort for the given size
            /// </summary>
            /// <param name="size">Size of bit vectors</param>
            /// <returns>The bit vector sort for the given size</returns>
            public static BitVectorsSort GetSort(long size)
            {
                if (_sortCache.ContainsKey(size))
                {
                    return _sortCache[size];
                }
                else
                {
                    _sortCache[size] = new BitVectorsSort(size);
                    return _sortCache[size];
                }
            }
        }

        /// <summary>
        /// This theory's name
        /// </summary>
        public SmtIdentifier Name { get; } = BitVectorsTheoryId;

        #region Deprecated
        public IReadOnlyDictionary<SmtIdentifier, IApplicable> Functions { get; }
        #endregion

        /// <summary>
        /// The primary (i.e., non-indexed) sort symbols (e.g., "BitVec")
        /// </summary>
        public IReadOnlySet<SmtIdentifier> PrimarySortSymbols { get; }

        /// <summary>
        /// The primary (i.e., non-indexed) function symbols
        /// </summary>
        public IReadOnlySet<SmtIdentifier> PrimaryFunctionSymbols { get; }

        /// <summary>
        /// Constructs an instance of the theory of bit vectors
        /// </summary>
        /// <param name="core">Reference to the core theory</param>
        private SmtBitVectorsTheory(SmtCoreTheory core)
        {
            SmtSort b = core.Sorts[BoolSortId.Name];

            SmtSourceBuilder sb = new(this);
            var bv0 = new SmtSort.WildcardSort(new(new SmtIdentifier("BitVec", "*")));

            // Concatenation: output size is equal to sum of input sizes
            sb.AddFn("concat",
                r => ((BitVectorsSort)r.ReturnSort).Size
                  == (((BitVectorsSort)r.ArgumentSorts[0]).Size
                    + ((BitVectorsSort)r.ArgumentSorts[1]).Size),
                "Size of return sort is equal to sum of input sizes",
                r => BitVectorsSort.GetSort(((BitVectorsSort)r.ArgumentSorts[0]).Size
                    + ((BitVectorsSort)r.ArgumentSorts[1]).Size),
                bv0, bv0, bv0);

            // Unary operations returnting a bit vector of the same size
            sb.AddFn("bvnot", SmtSourceBuilder.NoRankValidation, null, SmtSourceBuilder.UseFirstArgumentSort, bv0, bv0);
            sb.AddFn("bvneg", SmtSourceBuilder.NoRankValidation, null, SmtSourceBuilder.UseFirstArgumentSort, bv0, bv0);

            string argSortsEqualCmt = "Size of inputs must be the same";

            // Binary operations returning a bit vector of the same size
            sb.AddFn("bvand", SmtSourceBuilder.CheckArgumentSortsEqual, argSortsEqualCmt, SmtSourceBuilder.UseFirstArgumentSort, bv0, bv0, bv0);
            sb.AddFn("bvor", SmtSourceBuilder.CheckArgumentSortsEqual, argSortsEqualCmt, SmtSourceBuilder.UseFirstArgumentSort, bv0, bv0, bv0);
            sb.AddFn("bvadd", SmtSourceBuilder.CheckArgumentSortsEqual, argSortsEqualCmt, SmtSourceBuilder.UseFirstArgumentSort, bv0, bv0, bv0);
            sb.AddFn("bvmul", SmtSourceBuilder.CheckArgumentSortsEqual, argSortsEqualCmt, SmtSourceBuilder.UseFirstArgumentSort, bv0, bv0, bv0);
            sb.AddFn("bvudiv", SmtSourceBuilder.CheckArgumentSortsEqual, argSortsEqualCmt, SmtSourceBuilder.UseFirstArgumentSort, bv0, bv0, bv0);
            sb.AddFn("bvurem", SmtSourceBuilder.CheckArgumentSortsEqual, argSortsEqualCmt, SmtSourceBuilder.UseFirstArgumentSort, bv0, bv0, bv0);
            sb.AddFn("bvshl", SmtSourceBuilder.CheckArgumentSortsEqual, argSortsEqualCmt, SmtSourceBuilder.UseFirstArgumentSort, bv0, bv0, bv0);
            sb.AddFn("bvlshr", SmtSourceBuilder.CheckArgumentSortsEqual, argSortsEqualCmt, SmtSourceBuilder.UseFirstArgumentSort, bv0, bv0, bv0);

            // Comparison: argument sizes the same, and returns Boolean
            sb.AddFn("bvult", SmtSourceBuilder.CheckArgumentSortsEqual, argSortsEqualCmt, SmtSourceBuilder.UseReturnSort, b, bv0, bv0);

            // Extract: must be constructed on-the-fly
            sb.AddOnTheFlyFn("extract");

            Functions = sb.Functions;
            PrimaryFunctionSymbols = sb.PrimaryFunctionSymbols;
            PrimarySortSymbols = new HashSet<SmtIdentifier>() { BitVectorSortPrimaryId };
        }

        /// <summary>
        /// Looks up a sort symbol in this theory
        /// </summary>
        /// <param name="sortId">The sort ID</param>
        /// <param name="resolvedSort">The resolved sort</param>
        /// <returns>True if successfully gotten</returns>
        public bool TryGetSort(SmtSortIdentifier sortId, [NotNullWhen(true)] out SmtSort? resolvedSort)
        {
            if (sortId.Arity == 0 && sortId.Name.Symbol == "BitVec")
            {
                if (sortId.Name.Indices.Length != 1 || !sortId.Name.Indices[0].NumeralValue.HasValue) {
                    // Log a more helpful message? But where?
                    resolvedSort = default;
                    return false;
                }
                var val = sortId.Name.Indices[0].NumeralValue!.Value;
                if (val <= 0)
                {
                    resolvedSort = default;
                    return false;
                }

                resolvedSort = BitVectorsSort.GetSort(val);
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
            //
            // This needs to be constructed on the fly
            //
            if (functionId.Symbol == "extract")
            {
                if (functionId.Indices.Length != 2 ||
                    !functionId.Indices[0].NumeralValue.HasValue ||
                    functionId.Indices[0].NumeralValue!.Value < 0 ||
                    !functionId.Indices[1].NumeralValue.HasValue ||
                    functionId.Indices[1].NumeralValue!.Value < 0 ||
                    functionId.Indices[0].NumeralValue!.Value < functionId.Indices[1].NumeralValue!.Value)
                {
                    resolvedFunction = new SmtFunction(
                        new SmtIdentifier("extract", "i", "j"),
                        this,
                        new SmtFunctionRank(
                            new SmtSort.GenericSort(new(new SmtIdentifier("BitVec", "n"))),
                            new SmtSort.GenericSort(new(new SmtIdentifier("BitVec", "m"))))
                        {
                            ValidationComment = "i,j,m,n are numerals, m > i >= j > 0, n = i - j + 1"
                        });
                    return true; // No rank will be resolved, but we show a decent message
                }

                long i = functionId.Indices[0].NumeralValue!.Value;
                long j = functionId.Indices[1].NumeralValue!.Value;

                resolvedFunction = new SmtFunction(
                    functionId,
                    this,
                    new SmtFunctionRank(
                        BitVectorsSort.GetSort(i - j + 1),
                        new SmtSort.WildcardSort(new(new SmtIdentifier("BitVec", "*")))
                        )
                    {
                        Validator = r => ((BitVectorsSort)r.ArgumentSorts[0]).Size > i,
                        ValidationComment = "Size of input must be greater than " + i
                    });
                return true;
            }
            else
            {
                return Functions.TryGetValue(functionId, out resolvedFunction);
            }
        }
    }
}
