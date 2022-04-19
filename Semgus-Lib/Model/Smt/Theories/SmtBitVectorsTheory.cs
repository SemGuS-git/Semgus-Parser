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
                                                                      new SmtIdentifier.Index(size))))
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
        public IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions { get; }
        public IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts
            => throw new NotImplementedException();
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

            Dictionary<SmtIdentifier, SmtFunction> fd = new();
            void cf(string name, Func<SmtFunctionRank, bool> val, string? valCmt, Func<SmtFunctionRank, SmtSort> retCalc, SmtSort ret, params SmtSort[] args)
            {
                SmtIdentifier id = new(name);
                if (fd.TryGetValue(id, out SmtFunction? fun))
                {
                    fun.AddRankTemplate(new SmtFunctionRank(ret, args) { Validator = val, ReturnSortDeriver = retCalc, ValidationComment = valCmt });
                }
                else
                {
                    fd.Add(id, new SmtFunction(id, this, new SmtFunctionRank(ret, args) { Validator = val, ReturnSortDeriver = retCalc, ValidationComment = valCmt }));
                }
            }

            PrimarySortSymbols = new HashSet<SmtIdentifier>() { BitVectorSortPrimaryId };

            var bv0 = new SmtSort.WildcardSort(new(new SmtIdentifier("BitVec", new SmtIdentifier.Index("*"))));

            // Concatenation: output size is equal to sum of input sizes
            cf("concat",
                r => ((BitVectorsSort)r.ReturnSort).Size
                  == (((BitVectorsSort)r.ArgumentSorts[0]).Size
                    + ((BitVectorsSort)r.ArgumentSorts[1]).Size),
                "Size of return sort is equal to sum of input sizes",
                r => BitVectorsSort.GetSort(((BitVectorsSort)r.ArgumentSorts[0]).Size
                    + ((BitVectorsSort)r.ArgumentSorts[1]).Size),
                bv0, bv0, bv0);

            Func<SmtFunctionRank, bool> noVal = r => true;
            Func<SmtFunctionRank, SmtSort> firstArgSort = r => r.ArgumentSorts[0];

            // Unary operations returnting a bit vector of the same size
            cf("bvnot", noVal, null, firstArgSort, bv0, bv0);
            cf("bvneg", noVal, null, firstArgSort, bv0, bv0);

            Func<SmtFunctionRank, bool> argSortsEqual = r => r.ArgumentSorts[0] == r.ArgumentSorts[1];
            string argSortsEqualCmt = "Size of inputs must be the same";

            // Binary operations returning a bit vector of the same size
            cf("bvand", argSortsEqual, argSortsEqualCmt, firstArgSort, bv0, bv0, bv0);
            cf("bvor", argSortsEqual, argSortsEqualCmt, firstArgSort, bv0, bv0, bv0);
            cf("bvadd", argSortsEqual, argSortsEqualCmt, firstArgSort, bv0, bv0, bv0);
            cf("bvmul", argSortsEqual, argSortsEqualCmt, firstArgSort, bv0, bv0, bv0);
            cf("bvudiv", argSortsEqual, argSortsEqualCmt, firstArgSort, bv0, bv0, bv0);
            cf("bvurem", argSortsEqual, argSortsEqualCmt, firstArgSort, bv0, bv0, bv0);
            cf("bvshl", argSortsEqual, argSortsEqualCmt, firstArgSort, bv0, bv0, bv0);
            cf("bvshr", argSortsEqual, argSortsEqualCmt, firstArgSort, bv0, bv0, bv0);

            // Comparison: argument sizes the same, and returns Boolean
            cf("bvult", argSortsEqual, argSortsEqualCmt, r => r.ReturnSort, b, bv0, bv0);

            Functions = fd;
            var primary = new HashSet<SmtIdentifier>(fd.Keys);
            primary.Add(new SmtIdentifier("extract"));
            PrimaryFunctionSymbols = primary;
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
        public bool TryGetFunction(SmtIdentifier functionId, [NotNullWhen(true)] out SmtFunction? resolvedFunction)
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
                        new SmtIdentifier("extract", new SmtIdentifier.Index("i"), new SmtIdentifier.Index("j")),
                        this,
                        new SmtFunctionRank(
                            new SmtSort.GenericSort(new(new SmtIdentifier("BitVec", new SmtIdentifier.Index("n")))),
                            new SmtSort.GenericSort(new(new SmtIdentifier("BitVec", new SmtIdentifier.Index("m")))))
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
                        new SmtSort.WildcardSort(new(new SmtIdentifier("BitVec", new SmtIdentifier.Index("*"))))
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
