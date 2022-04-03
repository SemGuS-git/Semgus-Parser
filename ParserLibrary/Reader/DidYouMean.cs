using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// Suggestion generator based on edit distance between identifiers
    /// </summary>
    internal class DidYouMean : ISuggestionGenerator
    {
        /// <summary>
        /// Maximum number of suggestions to provide
        /// </summary>
        private const int SuggestionCount = 3;

        /// <summary>
        /// Maximum distance to consider
        /// </summary>
        private const int MaxEditDistanceForSimilarity = 3;

        /// <summary>
        /// Get suggestions for functions similar to the given identifier
        /// </summary>
        /// <param name="id">Seed identifier</param>
        /// <param name="context">Current SMT context</param>
        /// <param name="arity">Arity of function to suggest</param>
        /// <returns>Enumeration of suggested function identifiers</returns>
        public IEnumerable<SmtIdentifier> GetFunctionSuggestions(SmtIdentifier id, SmtContext context, int arity)
        {
            var fns = context.Functions
                .Where(fn => arity == -1 || fn.RankTemplates.Any(rt => rt.Arity == arity))
                .Select(fn => new { fn.Name, Distance = ComputeEditDistance(id.Symbol, fn.Name.Symbol) })
                .Where(p => p.Distance <= MaxEditDistanceForSimilarity)
                .OrderBy(p => p.Distance)
                .Take(SuggestionCount)
                .Select(p => p.Name)
                .ToList();

            if (fns.Count == 0 && arity != -1)
            {
                return GetFunctionSuggestions(id, context, -1);
            }
            else
            {
                return fns;
            }
        }

        /// <summary>
        /// Get suggestions for variables or constants similar to the given identifier
        /// </summary>
        /// <param name="id">Seed identifier</param>
        /// <param name="context">Current SMT context</param>
        /// <param name="scope">Current SMT scope</param>
        /// <returns>Enumeration of suggested variable or constant identifiers</returns>
        public IEnumerable<SmtIdentifier> GetVariableSuggestions(SmtIdentifier id, SmtContext context, SmtScope scope)
        {
            // Check locals first
            var locals = scope.Bindings
                    .Select(vb => (Id: vb.Id, Distance: ComputeEditDistance(id.Symbol, vb.Id.Symbol)))
                    .Where(p => p.Distance <= MaxEditDistanceForSimilarity);

            var globals = context.Functions
                .Where(fn => fn.RankTemplates.Any(rt => rt.Arity == 0))
                .Select(f => (Id: f.Name, Distance: ComputeEditDistance(id.Symbol, f.Name.Symbol)))
                .Where(p => p.Distance <= MaxEditDistanceForSimilarity);

            return locals.Concat(globals)
                .OrderBy(p => p.Distance)
                .Take(SuggestionCount)
                .Select(p => p.Id);
        }

        /// <summary>
        /// Computes the edit distance between two strings.
        /// Based on the Wagner-Fischer algorithm: https://en.wikipedia.org/wiki/Wagner%E2%80%93Fischer_algorithm
        ///    and extended to the Damerau–Levenshtein distance
        /// </summary>
        /// <param name="a">First string</param>
        /// <param name="b">Second string</param>
        /// <returns>Edit distance between strings</returns>
        private static int ComputeEditDistance(string a, string b)
        {
            var distances = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i < a.Length; ++i)
            {
                distances[i + 1, 0] = i + 1;
            }
            for (int j = 0; j < b.Length; ++j)
            {
                distances[0, j + 1] = j + 1;
            }
            for (int j = 0; j < b.Length; ++j)
            {
                for (int i = 0; i < a.Length; ++i)
                {
                    int substCost;
                    if (a[i] == b[j])
                    {
                        substCost = 0;
                    }
                    else
                    {
                        substCost = 1;
                    }

                    distances[i + 1, j + 1] = Math.Min(distances[i, j + 1] + 1,
                                                   Math.Min(distances[i + 1, j] + 1,
                                                        distances[i, j] + substCost));
                    if (i > 0 && j > 0 && a[i] == b[j - 1] && a[i - 1] == b[j])
                    {
                        distances[i + 1, j + 1] = Math.Min(distances[i + 1, j + 1],
                                                           distances[i - 1, j - 1] + 1);
                    }
                }
            }
            return distances[a.Length, b.Length];
        }
    }
}
