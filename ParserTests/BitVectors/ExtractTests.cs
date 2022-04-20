using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Semgus.Parser.Tests.NullableAssertions;

using Xunit;
using Semgus.Model.Smt.Theories;

namespace Semgus.Parser.Tests.BitVectors
{
    /// <summary>
    /// Tests related to the "extract" function. This function is special because
    /// it is constructed on-the-fly, rather than statically
    /// </summary>
    public class ExtractTests
    {
        /// <summary>
        /// The name of the function returned when an invalid (_ extract i j) form is given
        /// </summary>
        private static readonly SmtIdentifier InvalidName = new("extract",
                                                                new SmtIdentifier.Index("i"),
                                                                new SmtIdentifier.Index("j"));

        [Fact]
        public void TrueForWrongArguments()
        {
            SmtContext ctx = new();
            AssertTrue(ctx.TryGetFunctionDeclaration(new("extract"), out SmtFunction? function));
            Assert.Equal(InvalidName, function.Name);
            Assert.Single(function.RankTemplates);
        }

        [Theory]
        // j needs to be smaller than i
        [InlineData(0, 1)]
        // Not an integer
        [InlineData("Not", 1)]
        [InlineData(1, "Not")]
        [InlineData("2", "1")]
        // Must be 0 or greater
        [InlineData(-1, 7)]
        [InlineData(7, -45)]
        [InlineData(-24, -42)]
        public void TrueForWrongIndices(dynamic i, dynamic j)
        {
            SmtContext ctx = new();
            SmtIdentifier fnId = new("extract", new SmtIdentifier.Index(i), new SmtIdentifier.Index(j));
            AssertTrue(ctx.TryGetFunctionDeclaration(fnId, out SmtFunction? function));
            Assert.Equal(InvalidName, function.Name);
            Assert.Single(function.RankTemplates);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(7, 1)]
        [InlineData(0, 0)]
        [InlineData(3, 0)]
        public void TrueForRightIndices(long i, long j)
        {
            SmtContext ctx = new();
            SmtIdentifier fnId = new("extract", new SmtIdentifier.Index(i), new SmtIdentifier.Index(j));
            AssertTrue(ctx.TryGetFunctionDeclaration(fnId, out SmtFunction? function));
            Assert.Single(function.RankTemplates);
            var rank = function.RankTemplates.First();

            var ret = Assert.IsType<SmtBitVectorsTheory.BitVectorsSort>(rank.ReturnSort);
            Assert.Equal(i - j + 1, ret.Size);
        }

        [Theory]
        [InlineData(0, 0, 1)]
        [InlineData(7, 0, 8)]
        [InlineData(7, 5, 11)]
        public void CanResolve(long i, long j, long m)
        {
            long n = i - j + 1;
            SmtContext ctx = new();
            SmtIdentifier fnId = new("extract", new SmtIdentifier.Index(i), new SmtIdentifier.Index(j));
            AssertTrue(ctx.TryGetFunctionDeclaration(fnId, out SmtFunction? function));

            var argSort = SmtBitVectorsTheory.BitVectorsSort.GetSort(m);
            var retSort = SmtBitVectorsTheory.BitVectorsSort.GetSort(n);

            // First, try without specifying the return sort
            AssertTrue(function.TryResolveRank(out var rank, default, argSort));
            Assert.Single(rank.ArgumentSorts);
            Assert.Equal(argSort, rank.ArgumentSorts[0]);
            Assert.Equal(retSort, rank.ReturnSort);

            // Then, with specifying the return sort
            AssertTrue(function.TryResolveRank(out rank, retSort, argSort));
            Assert.Single(rank.ArgumentSorts);
            Assert.Equal(argSort, rank.ArgumentSorts[0]);
            Assert.Equal(retSort, rank.ReturnSort);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(7, 0, 4)]
        [InlineData(7, 5, 6)]
        [InlineData(7, 4, -6)]
        [InlineData(7, 3, 2, 12)] // Explicit return sort is wrong
        public void CannotResolve(long i, long j, long m, long? np = default)
        {
            long n = np ?? i - j + 1;
            SmtContext ctx = new();
            SmtIdentifier fnId = new("extract", new SmtIdentifier.Index(i), new SmtIdentifier.Index(j));
            AssertTrue(ctx.TryGetFunctionDeclaration(fnId, out SmtFunction? function));

            var argSort = SmtBitVectorsTheory.BitVectorsSort.GetSort(m);
            var retSort = SmtBitVectorsTheory.BitVectorsSort.GetSort(n);

            // First, try without specifying the return sort
            AssertFalse(function.TryResolveRank(out var _, default, argSort));

            // Then, with specifying the return sort
            AssertFalse(function.TryResolveRank(out _, retSort, argSort));
        }
    }
}
