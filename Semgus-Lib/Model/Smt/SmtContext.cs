using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Semgus.Model.Smt.Theories;

namespace Semgus.Model.Smt
{
    public class SmtContext
    {
        private readonly Stack<AssertionLevel> _assertionStack;

        private readonly HashSet<ISmtTheory> _theories;
        public IEnumerable<ISmtTheory> Theories => _theories;

        private AssertionLevel CurrentLevel => _assertionStack.Peek();


        public SmtContext()
        {
            _assertionStack = new Stack<AssertionLevel>();
            _assertionStack.Push(new AssertionLevel());

            _theories = new HashSet<ISmtTheory>()
            {
                SmtCoreTheory.Instance,
                SmtIntsTheory.Instance,
                SmtStringsTheory.Instance,
            };
        }

        /// <summary>
        /// All functions currently in the global context
        /// </summary>
        public IEnumerable<SmtFunction> Functions
        {
            get
            {
                foreach(var level in _assertionStack)
                {
                    foreach (var (_, function) in level.Functions)
                    {
                        yield return function;
                    }
                }

                foreach (var theory in _theories)
                {
                    foreach (var (_, function) in theory.Functions)
                    {
                        yield return function;
                    }
                }
            }
        }

        /// <summary>
        /// All sorts currently in the global context
        /// </summary>
        public IEnumerable<SmtSort> Sorts
        {
            get
            {
                foreach (var level in _assertionStack)
                {
                    foreach (var (_, sort) in level.Sorts)
                    {
                        yield return sort;
                    }
                }

                foreach (var theory in _theories)
                {
                    foreach (var (_, sort) in theory.Sorts)
                    {
                        yield return sort;
                    }
                }
            }
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
