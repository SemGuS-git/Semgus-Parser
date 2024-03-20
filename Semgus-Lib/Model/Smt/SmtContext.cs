using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt.Extensions;
using Semgus.Model.Smt.Sorts;
using Semgus.Model.Smt.Theories;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// Provides context about in-scope SMT objects
    /// </summary>
    public class SmtContext
    {
        /// <summary>
        /// The stack of active assertion levels
        /// </summary>
        private readonly Stack<AssertionLevel> _assertionStack;

        /// <summary>
        /// Underlying set of available theories
        /// </summary>
        private readonly HashSet<ISmtTheory> _theories;

        /// <summary>
        /// Enumeration of built-in theories
        /// </summary>
        public IEnumerable<ISmtTheory> Theories => _theories;

        /// <summary>
        /// Underlying set of available extensions
        /// </summary>
        private readonly HashSet<ISmtExtension> _extensions;

        /// <summary>
        /// Enumeration of built-in extensions
        /// </summary>
        public IEnumerable<ISmtExtension> Extensions => _extensions;

        /// <summary>
        /// Enumeration of all built-in sources
        /// </summary>
        public IEnumerable<ISmtBuiltInSource> BuiltInSources => _theories.Concat<ISmtBuiltInSource>(_extensions);

        /// <summary>
        /// The current assertion level
        /// </summary>
        private AssertionLevel CurrentLevel => _assertionStack.Peek();

        /// <summary>
        /// Creates a fresh SmtContext with default built-in sources
        /// </summary>
        public SmtContext()
        {
            _assertionStack = new Stack<AssertionLevel>();
            _assertionStack.Push(new AssertionLevel());

            _theories = new HashSet<ISmtTheory>()
            {
                SmtCoreTheory.Instance,
                SmtIntsTheory.Instance,
                SmtStringsTheory.Instance,
                SmtBitVectorsTheory.Instance,
                SmtArraysExTheory.Instance,
                SmtSequencesTheory.Instance
            };

            _extensions = new HashSet<ISmtExtension>()
            {
                SmtBitVectorsExtension.Instance
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

                foreach (var source in BuiltInSources)
                {
                    foreach (var fnId in source.PrimaryFunctionSymbols)
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

                foreach (var source in BuiltInSources)
                {
                    foreach (var sortId in source.PrimarySortSymbols)
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

            foreach (var source in BuiltInSources)
            {
                if (source.PrimaryFunctionSymbols.Contains(id) || source.PrimarySortSymbols.Contains(id))
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

            foreach (var source in BuiltInSources)
            {
                if (source.PrimarySortSymbols.Contains(id.Name))
                {
                    return true;
                }
            }

            return false;
        }

        public void AddFunctionDeclaration(IApplicable func)
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

            foreach (var sources in BuiltInSources)
            {
                if (sources.TryGetFunction(id, out function))
                {
                    return true;
                }
            }
            function = default;
            return false;
        }

        public bool TryGetSortDeclaration(SmtSortIdentifier id, [NotNullWhen(true)] out SmtSort? sort, [NotNullWhen(false)] out string? error)
        {
            foreach (var level in _assertionStack)
            {
                if (level.Sorts.ContainsKey(id.Name))
                {
                    sort = level.Sorts[id.Name];
                    return TryResolveSortParameters(id, sort, out sort, out error);
                }
            }

            foreach (var source in BuiltInSources)
            {
                if (source.TryGetSort(id, out sort))
                {
                    return TryResolveSortParameters(id, sort, out sort, out error);
                }
            }

            sort = default;
            error = "Unable to find a sort named " + id.Name;
            return false;
        }

        private bool TryResolveSortParameters(SmtSortIdentifier id, SmtSort candidate, [NotNullWhen(true)] out SmtSort? resolved, [NotNullWhen(false)] out string? error)
        {
            if (id.Arity != candidate.Arity)
            {
                resolved = default;
                error = $"Arity of sort {id.Name} ({candidate.Arity}) does not match given arity ({id.Arity})";
                return false;
            }

            if (candidate.Arity == 0 || !candidate.IsParametric)
            {
                resolved = candidate;
                error = default;
                return true;
            }

            List<SmtSort> resolvedSubsorts = new();
            foreach (var child in id.Parameters)
            {
                if (TryGetSortDeclaration(child, out var childSort, out error))
                {
                    resolvedSubsorts.Add(childSort);
                }
                else
                {
                    resolved = default;
                    error = $"Unable to resolve sort parameter {child.Name} in parametric sort {id.Name}: {error}";
                    return false;
                }
            }

            candidate.UpdateForResolvedParameters(resolvedSubsorts);

            resolved = candidate;
            error = "";
            return true;
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
            public Dictionary<SmtIdentifier, IApplicable> Functions { get; private set; }

            public AssertionLevel()
            {
                Sorts = new Dictionary<SmtIdentifier, SmtSort>();
                Functions = new Dictionary<SmtIdentifier, IApplicable>();
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
