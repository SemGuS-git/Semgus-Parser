using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Sorts
{
    /// <summary>
    /// A constructor for a datatype
    /// </summary>
    public class SmtDatatypeConstructor : ISmtConstructor, IApplicable
    {
        /// <summary>
        /// Creates a new datatype constructor
        /// </summary>
        /// <param name="name">This constructor name</param>
        /// <param name="parent">Datatype this constructor is associated with</param>
        /// <param name="children">Enumerable of children sorts</param>
        /// <param name="source">Originating SMT source</param>
        public SmtDatatypeConstructor(SmtIdentifier name, SmtDatatype parent, IEnumerable<SmtSort> children, ISmtSource source)
        {
            Name = name;
            Parent = parent;
            Children = children.ToList();
            Source = source;
        }

        /// <summary>
        /// Constructor name
        /// </summary>
        public SmtIdentifier Name { get; }
        
        /// <summary>
        /// Datatype this constructor is associated with
        /// </summary>
        public SmtDatatype Parent { get; }

        /// <summary>
        /// Constructor child sorts
        /// </summary>
        public IReadOnlyList<SmtSort> Children { get; }

        /// <summary>
        /// SMT source this constructor comes from
        /// </summary>
        public ISmtSource Source { get; }

        /// <summary>
        /// Gets an informative string about available ranks
        /// </summary>
        /// <returns>Rank information string</returns>
        public string GetRankHelp()
        {
            return $"({string.Join(' ', Children.Select(c => c.Name))}) -> {Parent.Name} [Constructor]";
        }

        /// <summary>
        /// Checks if there is a possible resolution for the given arity
        /// </summary>
        /// <param name="arity">Arity to check</param>
        /// <returns>True if there is a valid rank with the given arity</returns>
        public bool IsArityPossible(int arity)
        {
            return arity == Children.Count;
        }

        /// <summary>
        /// Tries to get an appropriate rank for the given parameter and return sorts
        /// </summary>
        /// <param name="ctx">The SMT context</param>
        /// <param name="rank">The resolved rank</param>
        /// <param name="returnSort">The return sort, if known</param>
        /// <param name="argumentSorts">The argument sorts</param>
        /// <returns>True if successfully resolved the rank</returns>
        public bool TryResolveRank(SmtContext ctx, [NotNullWhen(true)] out SmtFunctionRank? rank, SmtSort? returnSort, params SmtSort[] argumentSorts)
        {
            // TODO: handle parameterized datatypes
            if (returnSort is not null && returnSort != Parent)
            {
                rank = default;
                return false;
            }
            returnSort = Parent;

            if (Children.Count != argumentSorts.Length)
            {
                rank = default;
                return false;
            }

            for (int i = 0; i < argumentSorts.Length; ++i)
            {
                if (argumentSorts[i] != Children[i])
                {
                    rank = default;
                    return false;
                }
            }

            rank = new(returnSort, argumentSorts);
            return true;
        }
    }
}
