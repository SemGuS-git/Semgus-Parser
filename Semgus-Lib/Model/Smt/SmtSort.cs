using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    public abstract class SmtSort
    {
        public SmtSort(SmtSortIdentifier name)
        {
            Name = name;
        }

        public SmtSortIdentifier Name { get; }

        /// <summary>
        /// Does this sort have parameters?
        /// </summary>
        public bool IsParametric { get; protected set; } = false;

        /// <summary>
        /// Is this sort a sort parameter? A.k.a., needs to be resolved to a "real" sort?
        /// </summary>
        public bool IsSortParameter { get; protected set; } = false;

        private class GenericSort : SmtSort
        {
            public GenericSort(SmtSortIdentifier name) : base(name)
            { }
        }

        public class WildcardSort : SmtSort
        {
            public WildcardSort(SmtSortIdentifier name) : base(name)
            { }

            public bool Matches(SmtSort other)
            {
                if (Name.Name.Symbol == other.Name.Name.Symbol
                    && Name.Name.Indices.Length == other.Name.Name.Indices.Length)
                {
                    for (int ix = 0; ix < Name.Name.Indices.Length; ix++)
                    {
                        if (Name.Name.Indices[ix].StringValue != "*"
                            && Name.Name.Indices[ix].StringValue != other.Name.Name.Indices[ix].StringValue)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }
        }

        public class UniqueSortFactory
        {
            private class UniqueSort : SmtSort
            {
                public UniqueSort(string prefix, long counter) : base(new SmtSortIdentifier($"{prefix}{counter}"))
                {
                    IsSortParameter = true;
                }
            }
            
            private static long _counter = 1;

            private readonly string _prefix;
            public UniqueSortFactory(string name = "X")
            {
                _prefix = name;
                Sort = Next();
            }

            public SmtSort Sort { get; private set; }

            public SmtSort Next()
            {
                Sort = new UniqueSort(_prefix, Interlocked.Increment(ref _counter));
                return Sort;
            }
        }
    }
}
