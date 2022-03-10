using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    public class SmtContext
    {
        private readonly Stack<AssertionLevel> _assertionStack;

        private readonly HashSet<ISmtTheory> _theories;

        private AssertionLevel CurrentLevel => _assertionStack.Peek();

        public SmtContext()
        {
            _assertionStack = new Stack<AssertionLevel>();
            _assertionStack.Push(new AssertionLevel());

            _theories = new HashSet<ISmtTheory>()
            {
                Theories.SmtCoreTheory.Instance,
                Theories.SmtIntsTheory.Instance,
                Theories.SmtStringsTheory.Instance,
            };
        }

        public bool IsFunctionNameInUse(SmtIdentifier id)
        {
            // This iterates from top-down (from what I gather from the docs)
            foreach(var level in _assertionStack)
            {
                if (level.IsFunctionNameInUse(id))
                {
                    return true;
                }
            }

            foreach (var theory in _theories)
            {
                if (theory.Functions.ContainsKey(id) || theory.Sorts.ContainsKey(id))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSortNameInUse(SmtSortIdentifier id)
        {
            // This iterates from top-down (from what I gather from the docs)
            foreach (var level in _assertionStack)
            {
                if (level.IsFunctionNameInUse(id.Name))
                {
                    return true;
                }
            }

            foreach (var theory in _theories)
            {
                if (theory.Sorts.ContainsKey(id.Name))
                {
                    return true;
                }
            }

            return false;
        }

        public void AddFunctionDeclaration(SmtFunction func)
        {
            if (IsFunctionNameInUse(func.Name))
            {
                throw new InvalidOperationException("Redeclaration of identifier: " + func.Name);
            }

            CurrentLevel.Functions.Add(func.Name, func);
        }

        public void AddSortDeclaration(SmtSort sort)
        {
            if (IsSortNameInUse(sort.Name))
            {
                throw new InvalidOperationException("Redeclaration of identifier: " + sort.Name);
            }

            // Sorts are indexed by their primary name, not the parameters
            CurrentLevel.Sorts.Add(sort.Name.Name, sort);
        }

        /// <summary>
        /// Try to get a function declaration associated with the given ID
        /// </summary>
        /// <param name="id">The id to get a function declaration for</param>
        /// <param name="function">The function declaration if found</param>
        /// <returns>True if found a definition, false if name not in scope</returns>
        public bool TryGetFunctionDeclaration(SmtIdentifier id, [NotNullWhen(true)] out SmtFunction? function)
        {
            foreach (var level in _assertionStack)
            {
                if (level.Functions.ContainsKey(id))
                {
                    function = level.Functions[id];
                    return true;
                }
            }

            foreach (var theory in _theories)
            {
                if (theory.Functions.ContainsKey(id))
                {
                    function = theory.Functions[id];
                    return true;
                }
            }
            function = default;
            return false;
        }

        /// <summary>
        /// Returns a list of in-scope identifiers that are similar to the given id.
        /// This can be used to drive a "did-you-mean?"-type functionality.
        /// </summary>
        /// <param name="id">Identifier to check against</param>
        /// <returns>Enumeration of similar identifiers</returns>
        public IEnumerable<SmtIdentifier> GetSimilarFunctionNames(SmtIdentifier id)
        {
            foreach (var level in _assertionStack)
            {
                foreach (var candidate in level.Functions.Keys)
                {
                    if (IsIdSimilar(candidate, id))
                    {
                        yield return candidate;
                    }
                }
            }

            foreach (var theory in _theories)
            {
                foreach (var candidate in theory.Functions.Keys)
                {
                    if (IsIdSimilar(candidate, id))
                    {
                        yield return candidate;
                    }
                }
            }
        }

        private bool IsIdSimilar(SmtIdentifier a, SmtIdentifier b)
        {
            return string.Equals(a.Symbol.ToLowerInvariant(), b.Symbol.ToLowerInvariant())
                || ComputeEditDistance(a.Symbol, b.Symbol) <= MaxEditDistanceForSimilarity;
        }

        private const int MaxEditDistanceForSimilarity = 3;

        /// <summary>
        /// Computes the edit distance between two strings.
        /// Based on the Wagner-Fischer algorithm: https://en.wikipedia.org/wiki/Wagner%E2%80%93Fischer_algorithm
        /// </summary>
        /// <param name="a">First string</param>
        /// <param name="b">Second string</param>
        /// <returns>Edit distance between strings</returns>
        private int ComputeEditDistance(string a, string b)
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
                }
            }
            return distances[a.Length, b.Length];
        }

        [Obsolete("Use TryGetSortDeclaration instead.")]
        public SmtSort GetSortDeclaration(SmtIdentifier id)
        {
            if (TryGetSortDeclaration(new(id), out var sort))
            {
                return sort;
            }
            else
            {
                throw new InvalidOperationException("Sort not declared: " + id);
            }
        }

        public bool TryGetSortDeclaration(SmtSortIdentifier id, [NotNullWhen(true)] out SmtSort? sort)
        {
            foreach (var level in _assertionStack)
            {
                if (level.Sorts.ContainsKey(id.Name))
                {
                    sort = level.Sorts[id.Name];
                    return true;
                }
            }

            foreach (var theory in _theories)
            {
                if (theory.Sorts.ContainsKey(id.Name))
                {
                    sort = theory.Sorts[id.Name];
                    return true;
                }
            }

            sort = default;
            return false;
        }

        public SmtSort ResolveParameterizedSort(SmtSort parameterized, IList<SmtSort> arguments)
        {
            return parameterized; // TODO
        }

        public void Push()
        {
            _assertionStack.Push(new AssertionLevel());
        }

        public void Pop()
        {
            if (_assertionStack.Count == 1)
            {
                throw new InvalidOperationException("Unable to pop first assertion level from assertion stack.");
            }
        }

        private class AssertionLevel
        {
            public Dictionary<SmtIdentifier, SmtSort> Sorts { get; private set; }
            public Dictionary<SmtIdentifier, SmtFunction> Functions { get; private set; }

            public AssertionLevel()
            {
                Sorts = new Dictionary<SmtIdentifier, SmtSort>();
                Functions = new Dictionary<SmtIdentifier, SmtFunction>();
            }

            public bool IsFunctionNameInUse(SmtIdentifier id)
            {
                return Functions.ContainsKey(id);
            }
            public bool IsSortNameInUse(SmtSortIdentifier id)
            {
                return Sorts.ContainsKey(id.Name);
            }
        }
    }
}
