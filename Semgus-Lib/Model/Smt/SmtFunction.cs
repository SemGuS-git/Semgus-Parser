using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    public class SmtFunction : IApplicable
    {
        public SmtFunction(SmtIdentifier name, ISmtTheory theory, params SmtFunctionRank[] rankTemplates)
        {
            Name = name;
            Theory = theory;
            _rankTemplates = new List<SmtFunctionRank>(rankTemplates);
            _definitions = new Dictionary<SmtFunctionRank, SmtLambdaBinder>();
        }

        private readonly IDictionary<SmtFunctionRank, SmtLambdaBinder> _definitions;

        public void AddDefinition(SmtFunctionRank rank, SmtLambdaBinder definition)
        {
            _definitions[rank] = definition;
        }

        public SmtIdentifier Name { get; private set; }
        public ISmtTheory Theory { get; private set; }

        private readonly List<SmtFunctionRank> _rankTemplates;

        public void AddRankTemplate(SmtFunctionRank rank)
        {
            _rankTemplates.Add(rank);
        }

        public IReadOnlyCollection<SmtFunctionRank> RankTemplates => _rankTemplates;

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
