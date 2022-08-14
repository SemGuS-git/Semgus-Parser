using Semgus.Model.Smt.Terms;

using System.Diagnostics.CodeAnalysis;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// A function object
    /// </summary>
    public class SmtFunction : IApplicable
    {
        /// <summary>
        /// Constructs a new function object
        /// </summary>
        /// <param name="name">The function name</param>
        /// <param name="source">The function source (e.g., theory or extension)</param>
        /// <param name="rankTemplates">Templates for valid ranks</param>
        public SmtFunction(SmtIdentifier name, ISmtSource source, params SmtFunctionRank[] rankTemplates)
        {
            Name = name;
            Source = source;
            _rankTemplates = new List<SmtFunctionRank>(rankTemplates);
            _definitions = new Dictionary<SmtFunctionRank, SmtLambdaBinder>();
        }

        /// <summary>
        /// Mapping of ranks to definitions
        /// </summary>
        private readonly IDictionary<SmtFunctionRank, SmtLambdaBinder> _definitions;

        /// <summary>
        /// Adds a definition for the given rank
        /// </summary>
        /// <param name="rank">Rank to add definition for</param>
        /// <param name="definition">Function definition</param>
        public void AddDefinition(SmtFunctionRank rank, SmtLambdaBinder definition)
        {
            _definitions[rank] = definition;
        }

        /// <summary>
        /// Name of this function
        /// </summary>
        public SmtIdentifier Name { get; private set; }

        /// <summary>
        /// Source of this function, e.g. theory, extension, or user-defined
        /// </summary>
        public ISmtSource Source { get; private set; }

        /// <summary>
        /// List of valid rank templates
        /// </summary>
        private readonly List<SmtFunctionRank> _rankTemplates;

        /// <summary>
        /// Adds a valid rank template to this function
        /// </summary>
        /// <param name="rank">Rank template to add</param>
        public void AddRankTemplate(SmtFunctionRank rank)
        {
            _rankTemplates.Add(rank);
        }

        /// <summary>
        /// Gets a collection of all valid rank templates for this function
        /// </summary>
        public IReadOnlyCollection<SmtFunctionRank> RankTemplates => _rankTemplates;

        /// <summary>
        /// Attempts to resolve a concrete rank for the given function signature
        /// </summary>
        /// <param name="rank">The resolved rank</param>
        /// <param name="returnSort">Return sort of function call, if known</param>
        /// <param name="argumentSorts">Function call argument sorts</param>
        /// <returns>True if successfully resolved a concrete rank</returns>
        public bool TryResolveRank([NotNullWhen(true)] out SmtFunctionRank? rank, SmtSort? returnSort, params SmtSort[] argumentSorts)
        {
            var sameArity = _rankTemplates
                .Where(r => r.Arity == argumentSorts.Length);
            
            foreach (var template in sameArity)
            {
                if (TryResolveRank(out rank, template, returnSort, argumentSorts))
                {
                    return true; // We just pick the first one...maybe we need to check all templates and report ambiguities. TODO.
                }
            }
            rank = default;
            return false;
        }

        /// <summary>
        /// Attempts to resolve a concrete rank for the given function signature, from a single rank template
        /// </summary>
        /// <param name="rank">The resolved rank</param>
        /// <param name="template">The rank template to try</param>
        /// <param name="returnSort">Return sort of function call, if known</param>
        /// <param name="argumentSorts">Function call argument sorts</param>
        /// <returns>True if successfully resolved a concrete rank</returns>
        private bool TryResolveRank([NotNullWhen(true)] out SmtFunctionRank? rank, SmtFunctionRank template, SmtSort? returnSort, params SmtSort[] argumentSorts)
        {
            Dictionary<SmtSort, SmtSort> resolvedParameters = new();

            if (template.ArgumentSorts.Count != argumentSorts.Length)
            {
                rank = default;
                return false;
            }

            if (!template.ReturnSort.IsSortParameter && returnSort is not null && returnSort != template.ReturnSort)
            {
                rank = default;
                return false;
            }

            for (int ix = 0; ix < argumentSorts.Length; ++ix)
            {
                var templateSort = template.ArgumentSorts[ix];
                var concreteSort = argumentSorts[ix];

                if (templateSort.IsSortParameter)
                {
                    if (resolvedParameters.TryGetValue(templateSort, out var sort))
                    {
                        templateSort = sort;
                    }
                    else
                    {
                        resolvedParameters.Add(templateSort, concreteSort);
                        templateSort = concreteSort;
                    }
                }

                if (template.ArgumentSorts[ix] is SmtSort.WildcardSort wild
                    && wild.Matches(argumentSorts[ix]))
                {
                    // No-op
                }
                else if (templateSort != concreteSort)
                {
                    rank = default;
                    return false;
                }
            }

            if (template.HasUnresolvedSortParameters)
            {
                var retSort = template.ReturnSort;
                if (retSort.IsSortParameter)
                {
                    if (resolvedParameters.TryGetValue(retSort, out SmtSort? resRetSort))
                    {
                        // Case: the return value is a parameter we know about
                        retSort = resRetSort;
                    }
                    else if (returnSort is not null)
                    {
                        // Case: the return value is a free parameter, and we are given a return value
                        retSort = returnSort;
                    }
                    else
                    {
                        // Case: the return value is a free parameter, and we can't resolve it
                        rank = default;
                        return false;
                    }
                }

                if (returnSort is not null && returnSort != retSort)
                {
                    rank = default;
                    return false;
                }

                returnSort = retSort;
            }

            // By this point, we validated that the provided sorts fulfill the template
            rank = new SmtFunctionRank(returnSort ?? template.ReturnSort, argumentSorts);
            returnSort = template.ReturnSortDeriver(rank);
            if (returnSort != rank.ReturnSort)
            {
                rank = new SmtFunctionRank(returnSort, argumentSorts);
            }
            return template.Validator(rank);
        }

        /// <summary>
        /// Returns a string describing available ranks
        /// </summary>
        /// <returns>Descriptive string</returns>
        public string GetRankHelp()
        {
            string msg = $"\n  Available signatures: \n";
            foreach (var rankTemplate in RankTemplates)
            {
                msg += $"    - ({string.Join(' ', rankTemplate.ArgumentSorts.Select(s => s.Name))}) -> {rankTemplate.ReturnSort.Name}";
                if (rankTemplate.ValidationComment != null)
                {
                    msg += $"  [{rankTemplate.ValidationComment}]";
                }
                msg += "\n";
            }
            return msg;
        }

        /// <summary>
        /// Checks if there is a possible resolution for the given arity
        /// </summary>
        /// <param name="arity">Arity to check</param>
        /// <returns>True if there is a valid rank with the given arity</returns>
        public bool IsArityPossible(int arity)
        {
            return RankTemplates.Any(rt => rt.Arity == arity);
        }
    }
}
