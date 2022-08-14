using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// Holds information about bound variables in SMT terms
    /// </summary>
    public class SmtScope
    {
        /// <summary>
        /// The parent scope
        /// </summary>
        public SmtScope? Parent { get; private set; }

        /// <summary>
        /// Constructs a new scope with the given parent
        /// </summary>
        /// <param name="parent">Parent scope, or null if no parent</param>
        public SmtScope(SmtScope? parent)
        {
            Parent = parent;
            _variableBindings = new();
        }

        /// <summary>
        /// Gets a variable binding from this scope or the closest parent with the variable bound
        /// </summary>
        /// <param name="id">Identifier of variable to get</param>
        /// <param name="binding">Binding information about the variable</param>
        /// <returns>True if successfully gotten the variable binding information</returns>
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

        /// <summary>
        /// Attempts to add a new variable binding to this scope
        /// </summary>
        /// <param name="id">Variable identifier to add</param>
        /// <param name="sort">Sort of variable</param>
        /// <param name="bindingType">Type of binding</param>
        /// <param name="context">SMT context (for checking for shadowing theory symbols)</param>
        /// <param name="binding">The created binding</param>
        /// <param name="error">Reason for failure to add binding</param>
        /// <returns>True if successfully added, false if not</returns>
        public bool TryAddVariableBinding(SmtIdentifier id,
                                          SmtSort sort,
                                          SmtVariableBindingType bindingType,
                                          SmtContext context,
                                          [NotNullWhen(true)]  out SmtVariableBinding? binding,
                                          [NotNullWhen(false)] out string? error)
        {
            if (context.TryGetFunctionDeclaration(id, out var alreadyBound)
                && alreadyBound.Source is not SmtUserDefinedSource)
            {
                binding = default;
                error = $"cannot shadow theory symbol `{id}` from theory {alreadyBound.Source.Name}";
                return false;
            }

            binding = new SmtVariableBinding(id, sort, bindingType, this);
            if (!_variableBindings.TryAdd(id, binding))
            {
                error = $"identifer `{id}` already bound in this scope.";
                return false;
            }

            error = default;
            return true;
        }

        /// <summary>
        /// Bindings in this scope
        /// </summary>
        private readonly Dictionary<SmtIdentifier, SmtVariableBinding> _variableBindings;
    }
}
