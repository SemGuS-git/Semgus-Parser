using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// A rank is a particular non-empty sequence of sorts for a function symbol
    /// </summary>
    public class SmtFunctionRank
    {
        /// <summary>
        /// Constructs a new rank
        /// </summary>
        /// <param name="returnSort">Return sort for the rank</param>
        /// <param name="argumentSorts">Argument sorts for the rank</param>
        public SmtFunctionRank(SmtSort returnSort, params SmtSort[] argumentSorts)
        {
            ReturnSort = returnSort;
            ArgumentSorts = argumentSorts;
            Validator = r => true;
            ReturnSortDeriver = r => r.ReturnSort;
        }

        /// <summary>
        /// Validation function for checking this rank
        /// </summary>
        public Func<SmtFunctionRank, bool> Validator { get; init; }

        /// <summary>
        /// Comment about validation, displayed to the user
        /// </summary>
        public string? ValidationComment { get; init; }

        /// <summary>
        /// Computes a return sort for this rank
        /// </summary>
        public Func<SmtFunctionRank, SmtSort> ReturnSortDeriver { get; init; }

        /// <summary>
        /// Return sort for this rank
        /// </summary>
        public SmtSort ReturnSort { get; private set; }

        /// <summary>
        /// List of argument sorts for this rank
        /// </summary>
        public IReadOnlyList<SmtSort> ArgumentSorts { get; private set; }

        /// <summary>
        /// Number of arguments for this rank
        /// </summary>
        public int Arity => ArgumentSorts.Count;

        /// <summary>
        /// Whether or not this rank has parametric sorts
        /// </summary>
        public bool IsParametric => ReturnSort.IsParametric || ArgumentSorts.Any(s => s.IsParametric);

        /// <summary>
        /// Whether or not this rank has unresolved sort parameters
        /// </summary>
        public bool HasUnresolvedSortParameters => ReturnSort.IsSortParameter || ArgumentSorts.Any(s => s.IsSortParameter);

        /// <summary>
        /// Compares this rank to another
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return obj is SmtFunctionRank rank &&
                   ReturnSort == rank.ReturnSort &&
                   ArgumentSorts.SequenceEqual(rank.ArgumentSorts);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 19;
                foreach (var sort in ArgumentSorts)
                {
                    hash = hash * 31 + sort.GetHashCode();
                }
                hash = hash * 31 + ReturnSort.GetHashCode();
                return hash;
            }
        }
    }
}
