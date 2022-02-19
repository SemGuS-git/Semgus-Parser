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
        public SmtSort(SmtIdentifier name)
        {
            Name = name;
        }

        public SmtIdentifier Name { get; }

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
            public GenericSort(SmtIdentifier name) : base(name)
            { }
        }

        public class UniqueSortFactory
        {
            private class UniqueSort : SmtSort
            {
                public UniqueSort(string prefix, long counter) : base(new SmtIdentifier($"{prefix}{counter}"))
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
