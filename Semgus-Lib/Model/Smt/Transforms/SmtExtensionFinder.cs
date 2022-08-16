using Semgus.Model.Smt.Extensions;
using Semgus.Model.Smt.Terms;

namespace Semgus.Model.Smt.Transforms
{
    /// <summary>
    /// Gets all used extension functions in an SMT term
    /// </summary>
    public class SmtExtensionFinder : SmtTermWalker<ISet<SmtExtensionFinder.Extension>>
    {
        /// <summary>
        /// An extension function usage
        /// </summary>
        /// <param name="Function">Function</param>
        /// <param name="Rank">Rank</param>
        public record Extension(SmtFunction Function, SmtFunctionRank Rank);

        /// <summary>
        /// Creates a new extension finder
        /// </summary>
        public SmtExtensionFinder() : base(new HashSet<Extension>())
        {
        }

        /// <summary>
        /// Finds extensions used in the given term
        /// </summary>
        /// <param name="term">Term to search</param>
        /// <returns>Set of used extensions</returns>
        public static ISet<Extension> Find(SmtTerm term)
        {
            return term.Accept(new SmtExtensionFinder()).Item2;
        }

        /// <summary>
        /// Merges the upward hash sets into a single set
        /// </summary>
        /// <param name="root">Term where data is being merged</param>
        /// <param name="data">List of data to merge</param>
        /// <returns>Merged data</returns>
        protected override ISet<Extension> MergeData(SmtTerm root, IEnumerable<ISet<Extension>> data)
        {
            HashSet<Extension> hs = new();
            foreach (var d in data)
            {
                hs.UnionWith(d);
            }
            return hs;
        }

        /// <summary>
        /// Checks a function application call for extensions being used
        /// </summary>
        /// <param name="appl">The application term</param>
        /// <param name="arguments">The application arguments</param>
        /// <param name="up">The upward data from children</param>
        /// <returns>The application and the found extensions</returns>
        public override (SmtTerm, ISet<Extension>) OnFunctionApplication(SmtFunctionApplication appl, IReadOnlyList<SmtTerm> arguments, IReadOnlyList<ISet<Extension>> up)
        {
            ISet<Extension> merged = MergeData(appl, up);
            if (appl.Definition.Source is ISmtExtension && appl.Definition is SmtFunction function)
            {
                merged.Add(new Extension(function, appl.Rank));
            }

            return (appl, merged);
        }
    }
}
