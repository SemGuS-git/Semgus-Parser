using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// Base class for all term sorts
    /// </summary>
    public abstract class SmtSort
    {
        /// <summary>
        /// Constructs a new sort with the given name
        /// </summary>
        /// <param name="name">Sort name</param>
        public SmtSort(SmtSortIdentifier name)
        {
            Name = name;
        }

        /// <summary>
        /// Name of this sort
        /// </summary>
        public SmtSortIdentifier Name { get; }

        /// <summary>
        /// Does this sort have parameters that need to be resolved?
        /// </summary>
        public bool IsParametric { get; protected set; } = false;

        /// <summary>
        /// Is this sort a sort parameter? A.k.a., needs to be resolved to a "real" sort?
        /// </summary>
        public bool IsSortParameter { get; protected set; } = false;

        /// <summary>
        /// Number of sort parameters needed for this sort
        /// </summary>
        public int Arity { get; protected set; } = 0;

        /// <summary>
        /// Updates this sort for resolved parameters
        /// </summary>
        /// <param name="resolved">Resolved parameters. Should have same length as arity</param>
        public virtual void UpdateForResolvedParameters(IList<SmtSort> resolved) { }

        /// <summary>
        /// An arbitrary generic sort
        /// </summary>
        internal class GenericSort : SmtSort
        {
            /// <summary>
            /// Constructs a new generic sort
            /// </summary>
            /// <param name="name">The sort name</param>
            public GenericSort(SmtSortIdentifier name) : base(name)
            { }
        }

        /// <summary>
        /// A sort parameter that needs to be resolved to a real sort
        /// </summary>
        internal class UnresolvedParameterSort : SmtSort
        {
            /// <summary>
            /// Identifier that needs to be resolved
            /// </summary>
            public SmtSortIdentifier Identifier { get; }

            /// <summary>
            /// Creates a new unresolved sort. This is a placeholder for sort parameters to be resolved.
            /// </summary>
            /// <param name="identifier">Sort identifier to resolve</param>
            public UnresolvedParameterSort(SmtSortIdentifier identifier) : base(identifier)
            {
                Identifier = identifier;
                IsSortParameter = true;
                Arity = identifier.Arity;
            }
        }

        /// <summary>
        /// A sort containing wildcard parameters. Useful in rank templates.
        /// Indices in the sort name may be '*', which will match anything.
        /// </summary>
        public class WildcardSort : SmtSort
        {
            /// <summary>
            /// Constructs a new wildcard sort
            /// </summary>
            /// <param name="name">Sort name with wildcards</param>
            public WildcardSort(SmtSortIdentifier name) : base(name)
            { }

            /// <summary>
            /// Checks if this sort matches another sort (taking wildcards into account)
            /// </summary>
            /// <param name="other">The other sort</param>
            /// <returns>True if matches, false otherwise</returns>
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

        /// <summary>
        /// Factory for constructing unique sorts (e.g., for rank templates)
        /// </summary>
        public class UniqueSortFactory
        {
            /// <summary>
            /// Unique sorts that are different from any other
            /// </summary>
            private class UniqueSort : SmtSort
            {
                /// <summary>
                /// Constructs a new sort with the given prefix and counter
                /// </summary>
                /// <param name="prefix"></param>
                /// <param name="counter"></param>
                public UniqueSort(string prefix, long counter) : base(new SmtSortIdentifier($"{prefix}{counter}"))
                {
                    IsSortParameter = true;
                }
            }
            
            /// <summary>
            /// Counter for unique sorts. Changed after constructing a sort
            /// </summary>
            private static long _counter = 1;

            /// <summary>
            /// Prefix for this factory
            /// </summary>
            private readonly string _prefix;

            /// <summary>
            /// Constructs a new UniqueSortFactory with the given prefix
            /// </summary>
            /// <param name="name">Sort prefix to use</param>
            public UniqueSortFactory(string name = "X")
            {
                _prefix = name;
                Sort = Next();
            }

            /// <summary>
            /// The most recently constructed sort
            /// </summary>
            public SmtSort Sort { get; private set; }

            /// <summary>
            /// Constructs a new unique sort
            /// </summary>
            /// <returns>The constructed sort</returns>
            public SmtSort Next()
            {
                Sort = new UniqueSort(_prefix, Interlocked.Increment(ref _counter));
                return Sort;
            }
        }
    }
}
