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

        public bool IsNameInUse(SmtIdentifier id)
        {
            // This iterates from top-down (from what I gather from the docs)
            foreach(var level in _assertionStack)
            {
                if (level.IsNameInUse(id))
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

        public void AddFunctionDeclaration(SmtFunction func)
        {
            if (IsNameInUse(func.Name))
            {
                throw new InvalidOperationException("Redeclaration of identifier: " + func.Name);
            }

            CurrentLevel.Functions.Add(func.Name, func);
        }

        public void AddSortDeclaration(SmtSort sort)
        {
            if (IsNameInUse(sort.Name))
            {
                throw new InvalidOperationException("Redeclaration of identifier: " + sort.Name);
            }

            CurrentLevel.Sorts.Add(sort.Name, sort);
        }

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

        public SmtSort GetSortDeclaration(SmtIdentifier id)
        {
            foreach (var level in _assertionStack)
            {
                if (level.Sorts.ContainsKey(id))
                {
                    return level.Sorts[id];
                }
            }

            foreach (var theory in _theories)
            {
                if (theory.Sorts.ContainsKey(id))
                {
                    return theory.Sorts[id];
                }
            }

            throw new InvalidOperationException("Sort not declared: " + id);
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

            public bool IsNameInUse(SmtIdentifier id)
            {
                return Sorts.ContainsKey(id) || Functions.ContainsKey(id);
            }
        }
    }
}
