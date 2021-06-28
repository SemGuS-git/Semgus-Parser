using System.Collections.Generic;
using System.Linq;

namespace Semgus.Syntax
{
    public class VariableClosure
    {
        public VariableClosure Parent { get; }
        public IReadOnlyList<NonterminalTermDeclaration> Terms { get; }
        public IReadOnlyList<VariableDeclaration> Variables { get; }

        private readonly IReadOnlyDictionary<string, VariableDeclaration> _lookupTable;

        public VariableClosure Clone()
        {
            return new VariableClosure(Parent, Variables.ToList());
        }

        public VariableClosure(VariableClosure parent, IEnumerable<VariableDeclaration> variables)
        {
            Parent = parent;
            Variables = variables.ToList();
            _lookupTable = Variables.ToDictionary(v => v.Name);
        }

        public bool TryResolve(string name, out VariableDeclaration variable)
        {
            if (_lookupTable.TryGetValue(name, out variable)) return true;

            if (!(Parent is null)) return Parent.TryResolve(name, out variable);

            variable = default;
            return false;
        }

        public string PrintAllResolvableVariables()
        {
            var s = string.Join(" ", Variables.Select(v => v.Name));
            if (!(Parent is null)) s += " " + Parent.PrintAllResolvableVariables();
            return s;
        }
    }
}