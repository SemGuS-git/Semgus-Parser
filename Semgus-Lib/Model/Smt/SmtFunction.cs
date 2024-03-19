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
        /// Hook called when trying to get a definition and none found.
        /// Takes an SmtContext, this function and the desired rank as arguments and returns
        /// a definition, or null if not found. Note that the computed definition
        /// is not automatically cached - hooks must call AddDefinition if the
        /// definition should be kept.
        /// </summary>
        public Func<SmtContext, SmtFunction, SmtFunctionRank, SmtLambdaBinder?> DefinitionMissingHook { get; set; } = (_, _, _) => null;

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
        /// Gets a function definition for the given rank
        /// </summary>
        /// <param name="rank">Rank to get definition for</param>
        /// <param name="definition">The definition</param>
        /// <returns>True if successfully got definition</returns>
        public bool TryGetDefinition(SmtContext ctx, SmtFunctionRank rank, [NotNullWhen(true)] out SmtLambdaBinder? definition)
        {
            if (_definitions.TryGetValue(rank, out definition))
            {
                return true;
            }

            definition = DefinitionMissingHook(ctx, this, rank);
            return definition is not null;
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
        /// <param name="ctx">The SMT context</param>
        /// <param name="rank">The resolved rank</param>
        /// <param name="returnSort">Return sort of function call, if known</param>
        /// <param name="argumentSorts">Function call argument sorts</param>
        /// <returns>True if successfully resolved a concrete rank</returns>
        public bool TryResolveRank(SmtContext ctx, [NotNullWhen(true)] out SmtFunctionRank? rank, SmtSort? returnSort, params SmtSort[] argumentSorts)
        {
            var sameArity = _rankTemplates
                .Where(r => r.Arity == argumentSorts.Length);
            
            foreach (var template in sameArity)
            {
                if (TryResolveRank(ctx, out rank, template, returnSort, argumentSorts))
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
        /// <param name="ctx">The SMT context</param>
        /// <param name="rank">The resolved rank</param>
        /// <param name="template">The rank template to try</param>
        /// <param name="returnSort">Return sort of function call, if known</param>
        /// <param name="argumentSorts">Function call argument sorts</param>
        /// <returns>True if successfully resolved a concrete rank</returns>
        private bool TryResolveRank(SmtContext ctx, [NotNullWhen(true)] out SmtFunctionRank? rank, SmtFunctionRank template, SmtSort? returnSort, params SmtSort[] argumentSorts)
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
                        // Template sorts (that are a parameter) are allowed to have arity 0.
                        // The whole template will just be matched with the concrete sort.
                        // Otherwise, we want to make sure the arities match.
                        if (!(templateSort.IsSortParameter && templateSort.Arity == 0)
                            && templateSort.Arity != concreteSort.Arity)
                        {
                            rank = default;
                            return false;
                        }

                        // Recursively match the template and match parameter sorts
                        if (!TraverseAndMatchTemplate(templateSort, concreteSort, resolvedParameters))
                        {
                            rank = default;
                            return false;
                        }
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
                    TraverseAndResolveTemplate(ctx, retSort, resolvedParameters, out _);

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
        /// Traverses the template and resolves parameters inside it, from already resolved pieces
        /// </summary>
        /// <param name="ctx">The SMT context</param>
        /// <param name="template">The template to match</param>
        /// <param name="resolvedParameters">Dictionary of templates to resolved parameters</param>
        /// <param name="resolved">The resolved sort</param>
        /// <returns>True if successfully resolved</returns>
        internal static bool TraverseAndResolveTemplate(SmtContext ctx, SmtSort template, IDictionary<SmtSort, SmtSort> resolvedParameters, [NotNullWhen(true)] out SmtSort? resolved)
        {
            if (!template.IsSortParameter)
            {
                resolved = template;
                return true;
            }
            else if (resolvedParameters.TryGetValue(template, out resolved))
            {
                return true;
            }
            else if (template.Arity == 0)
            {
                // An unresolved parameter! Resolution failed.
                resolved = default;
                return false;
            }
            else
            {
                List<SmtSortIdentifier> paramIds = new();
                foreach (var parameter in template.Parameters)
                {
                    if (!TraverseAndResolveTemplate(ctx, parameter, resolvedParameters, out resolved))
                    {
                        resolved = default;
                        return false;
                    }
                    paramIds.Add(resolved.Name);
                }
                if (!ctx.TryGetSortDeclaration(new(template.Name.Name, paramIds.ToArray()), out var sort, out string? _))
                {
                    resolved = default;
                    return false;
                }
                resolvedParameters.Add(template, sort);
                resolved = sort;
                return true;
            }
        }

        /// <summary>
        /// Match a template sort and parameters against a concrete sort
        /// </summary>
        /// <param name="template">Template sort to match</param>
        /// <param name="concrete">Concrete sort to match against</param>
        /// <param name="resolvedParameters">Dictionary of resolved parameters</param>
        /// <returns>True if successfully resolves, false if not</returns>
        internal static bool TraverseAndMatchTemplate(SmtSort template, SmtSort concrete, IDictionary<SmtSort, SmtSort> resolvedParameters)
        {
            // Template can be a parameter, and concrete can be whatever
            if (template.IsSortParameter)
            {
                if (resolvedParameters.TryGetValue(template, out var resolved))
                {
                    if (resolved != concrete)
                    {
                        return false;
                    }
                }
                else
                {
                    resolvedParameters.Add(template, concrete);
                }

                if (template.Arity == 0 && concrete.Arity > 0)
                {
                    return true;
                }
            }

            // Otherwise, arities must match, so we can traverse
            if (template.Arity != concrete.Arity)
            {
                return false;
            }

            if (template.IsSortParameter)
            {
                foreach (var (tempParam, concParam) in template.Parameters.Zip(concrete.Parameters))
                {
                    if (!TraverseAndMatchTemplate(tempParam, concParam, resolvedParameters))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (template != concrete)
                {
                    return false;
                }
            }
            return true;
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
