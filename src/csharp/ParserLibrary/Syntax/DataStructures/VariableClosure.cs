using System;
using System.Collections.Generic;
using System.Linq;

namespace Semgus.Syntax {
    public class VariableClosure {
        public VariableClosure Parent { get; }
        public IReadOnlyList<NonterminalTermDeclaration> Terms { get; }
        public IReadOnlyList<VariableDeclaration> Variables { get; }

        private readonly IReadOnlyDictionary<string, VariableDeclaration> _lookupTable;

        public VariableClosure(VariableClosure parent, IReadOnlyList<VariableDeclaration> variables) {
            Parent = parent;
            Variables = variables;
            _lookupTable = Variables.ToDictionary(v => v.Name);
        }
        
        public bool TryResolve(string name, out VariableDeclaration variable) {
            if(_lookupTable.TryGetValue(name, out variable)) return true;
            
            if(!(Parent is null)) return Parent.TryResolve(name, out variable);
            
            variable = default;
            return false;   
        }
        
        public IEnumerable<VariableDeclaration> Input() => Variables.Where(v=>v.Usage== VariableDeclaration.SemanticUsage.Input);
        public IEnumerable<VariableDeclaration> Output() => Variables.Where(v=>v.Usage== VariableDeclaration.SemanticUsage.Output);
        public IEnumerable<VariableDeclaration> Auxiliary() => Variables.Where(v=>v.Usage== VariableDeclaration.SemanticUsage.Auxiliary);

        public string PrintAllResolvableVariables() {
            var s = string.Join(" ",Variables.Select(v=>v.Name));
            if(!(Parent is null)) s += " " + Parent.PrintAllResolvableVariables();
            return s;
        }
    }
}