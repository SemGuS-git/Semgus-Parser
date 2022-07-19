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
                SmtBitVectorsTheory.Instance
            };
        }

        /// <summary>
        /// All primary function symbols currently in the global context
        /// </summary>
        public IEnumerable<SmtIdentifier> Functions
        {
            get
            {
                foreach(var level in _assertionStack)
                {
                    foreach (var (fnId, _) in level.Functions)
                    {
                        yield return fnId;
                    }
                }

                foreach (var theory in _theories)
                {
                    foreach (var fnId in theory.PrimaryFunctionSymbols)
                    {
                        yield return fnId;
                    }
                }
            }
        }

        /// <summary>
        /// All primary sort symbols currently in the global context
        /// </summary>
        public IEnumerable<SmtIdentifier> Sorts
        {
            get
            {
                foreach (var level in _assertionStack)
                {
                    foreach (var (sortId, _) in level.Sorts)
                    {
                        yield return sortId;
                    }
                }

                foreach (var theory in _theories)
                {
                    foreach (var sortId in theory.PrimarySortSymbols)
                    {
                        yield return sortId;
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
                if (theory.PrimaryFunctionSymbols.Contains(id) || theory.PrimarySortSymbols.Contains(id))
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
                if (theory.PrimarySortSymbols.Contains(id.Name))
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
        public bool TryGetFunctionDeclaration(SmtIdentifier id, [NotNullWhen(true)] out IApplicable? function)
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
                if (theory.TryGetFunction(id, out function))
                {
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
                if (theory.TryGetSort(id, out sort))
                {
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
            _assertionStack.Pop();
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
