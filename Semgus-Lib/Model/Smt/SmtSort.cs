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
        public bool IsParametric { get; protected set; } = false;

        public static SmtSort BoolSort = GetOrCreateSort("Bool");
        public static SmtSort IntSort = GetOrCreateSort("Int");
        public static SmtSort StringSort = GetOrCreateSort("String");

        public static SmtSort GetOrCreateSort(string id)
        {
            return GetOrCreateSort(new SmtIdentifier(id));
        }

        public static SmtSort GetOrCreateSort(SmtIdentifier id)
        {
            return new GenericSort(id);
        }

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
                    IsParametric = true;
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
                return new UniqueSort(_prefix, Interlocked.Increment(ref _counter));
            }
        }
    }
}
