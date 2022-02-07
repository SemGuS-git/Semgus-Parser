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

        public void AddVariableBinding(SmtIdentifier id, SmtSort sort, BindingType bindingType)
        {
            if (!_variableBindings.TryAdd(id, new SmtVariableBinding(id, sort, bindingType, this)))
            {
                throw new InvalidOperationException($"Identifer {id.Symbol} already bound in this scope.");
            }
            // TODO: it's an error to shadow theory symbols
        }

        private readonly Dictionary<SmtIdentifier, SmtVariableBinding> _variableBindings;

        public record SmtVariableBinding(SmtIdentifier Id, SmtSort Sort, BindingType BindingType, SmtScope DeclaringScope);

        public enum BindingType
        {
            Free,
            Bound,
            Existential,
            Universal
        }

    }
}
