using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    public class SmtScope
    {
        public SmtScope? Parent { get; private set; }
        public SmtScope(SmtScope? parent)
        {
            Parent = parent;
            _variableBindings = new();
        }

        public bool TryGetVariableBinding(SmtIdentifier id, [NotNullWhen(true)] out SmtVariableBinding? binding)
        {
            if (_variableBindings.TryGetValue(id, out binding)) {
                return true;
            }
            else if (Parent is not null)
            {
                return Parent.TryGetVariableBinding(id, out binding);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// All variable bindings local to this scope
        /// </summary>
        public IReadOnlyCollection<SmtVariableBinding> LocalBindings { get => _variableBindings.Values; }

        /// <summary>
        /// All variable bindings in this scope and parent scopes
        /// </summary>
        public IEnumerable<SmtVariableBinding> Bindings
            => LocalBindings.Concat(Parent?.Bindings ?? Enumerable.Empty<SmtVariableBinding>());

        public SmtVariableBinding AddVariableBinding(SmtIdentifier id, SmtSort sort, SmtVariableBindingType bindingType)
        {
            var binding = new SmtVariableBinding(id, sort, bindingType, this);
            if (!_variableBindings.TryAdd(id, binding))
            {
                throw new InvalidOperationException($"Identifer {id.Symbol} already bound in this scope.");
            }
            // TODO: it's an error to shadow theory symbols
            return binding;
        }

        private readonly Dictionary<SmtIdentifier, SmtVariableBinding> _variableBindings;
    }
}
