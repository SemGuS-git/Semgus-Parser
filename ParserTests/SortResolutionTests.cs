using Semgus.Model.Smt;
using Semgus.Model.Smt.Theories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Semgus.Parser.Tests
{
    public class SortResolutionTests
    {
        /// <summary>
        /// Test that a template sort will be recursively matched against a concrete sort,
        /// and that all the sub-pieces of the template will be resolved.
        /// </summary>
        [Fact]
        public void TemplatesRecursivelyResolved()
        {
            IDictionary<SmtSort, SmtSort> resolved = new Dictionary<SmtSort, SmtSort>();

            var usf = new SmtSort.UniqueSortFactory();
            var tix1 = usf.Next();
            var val1 = usf.Next();
            var tix2 = usf.Next();

            var t2 = new SmtArraysExTheory.ArraySort(tix1, val1);
            var t1 = new SmtArraysExTheory.ArraySort(tix2, t2);


            var pix1 = new SmtSort.GenericSort(new("Proc1"));
            var pvl1 = new SmtSort.GenericSort(new("PVal1"));
            var pix2 = new SmtSort.GenericSort(new("Proc2"));
            var p2 = new SmtArraysExTheory.ArraySort(pix1, pvl1);
            var p1 = new SmtArraysExTheory.ArraySort(pix2, p2);

            Assert.True(SmtFunction.TraverseAndMatchTemplate(t1, p1, resolved));

            Assert.Equal(5, resolved.Count);
            Assert.Equal(pix1, resolved[tix1]);
            Assert.Equal(pix2, resolved[tix2]);
            Assert.Equal(pvl1, resolved[val1]);
            Assert.Equal(p2, resolved[t2]);
            Assert.Equal(p1, resolved[t1]);
        }

        /// <summary>
        /// Tests that single parametric sorts will be matched against sorts with higher arity.
        /// E.g., a template X1 can match against (Array Int Int) fine
        /// </summary>
        [Fact]
        public void SingleTemplatesMatchedAgainstParametric()
        {
            IDictionary<SmtSort, SmtSort> resolved = new Dictionary<SmtSort, SmtSort>();

            var usf = new SmtSort.UniqueSortFactory();

            var top1 = usf.Next();
            var subIx = usf.Next();
            var subVal = usf.Next();
            var top2 = new SmtArraysExTheory.ArraySort(subIx, subVal);

            var pix1 = new SmtSort.GenericSort(new("Proc1"));
            var pvl1 = new SmtSort.GenericSort(new("PVal1"));
            var pix2 = new SmtSort.GenericSort(new("Proc2"));
            var p2 = new SmtArraysExTheory.ArraySort(pix1, pvl1);
            var p1 = new SmtArraysExTheory.ArraySort(pix2, p2);

            Assert.True(SmtFunction.TraverseAndMatchTemplate(top1, p1, resolved));
            Assert.True(SmtFunction.TraverseAndMatchTemplate(top2, p1, resolved));

            Assert.Equal(4, resolved.Count);
            Assert.Equal(p1, resolved[top1]);
            Assert.Equal(p1, resolved[top2]);
            Assert.Equal(pix2, resolved[subIx]);
            Assert.Equal(p2, resolved[subVal]);

        }
    }
}
