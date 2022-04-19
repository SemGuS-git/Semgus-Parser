using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Semgus.Model.Smt.SmtCommonIdentifiers;

namespace Semgus.Model.Smt.Theories
{

    internal class SmtBitVectorsTheory : ISmtTheory
    {
        public static SmtBitVectorsTheory Instance { get; } = new(SmtCoreTheory.Instance);

        private sealed class BitVectorsSort : SmtSort
        {
            private static readonly IDictionary<long, BitVectorsSort> _sortCache = new Dictionary<long, BitVectorsSort>();

            public long Size { get; }
            private BitVectorsSort(long size) : base(new(new SmtIdentifier(BitVectorSortPrimaryId.Symbol,
                                                                      new SmtIdentifier.Index(size))))
            {
                Size = size;
            }
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

        public SmtIdentifier Name { get; } = BitVectorsTheoryId;
        public IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions { get; }
        public IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts
            => throw new NotImplementedException();

        public IReadOnlySet<SmtIdentifier> PrimarySortSymbols { get; }

        public IReadOnlySet<SmtIdentifier> PrimaryFunctionSymbols { get; }

        private SmtBitVectorsTheory(SmtCoreTheory core)
        {
            SmtSort i = SmtIntsTheory.Instance.Sorts[IntSortId.Name];
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
            PrimaryFunctionSymbols = new HashSet<SmtIdentifier>(fd.Keys);
        }

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

        public bool TryGetFunction(SmtIdentifier functionId, [NotNullWhen(true)] out SmtFunction? resolvedFunction)
            =>
                Functions.TryGetValue(functionId, out resolvedFunction);
    }
}
