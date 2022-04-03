using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt;

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// Holds a reference to the current SMT scope while parsing,
    /// as well as allows starting a new scope.
    /// </summary>
    public interface ISmtScopeProvider
    {
        /// <summary>
        /// Gets the current scope
        /// </summary>
        public SmtScope Scope { get; }

        /// <summary>
        /// Creates a new scope, which ends when the ISmtScopeContext is disposed
        /// </summary>
        /// <returns>The new scope</returns>
        public ISmtScopeContext CreateNewScope();

        /// <summary>
        /// Context interface that ends a scope when disposed
        /// </summary>
        public interface ISmtScopeContext : ISmtScopeProvider, IDisposable
        {
        }
    }

    /// <summary>
    /// Holds a reference to the current SMT scope while parsing
    /// </summary>
    internal class SmtScopeProvider : ISmtScopeProvider, ISmtScopeProvider.ISmtScopeContext
    {
        /// <summary>
        /// Backing scope field. This is null when no scopes have been created
        /// </summary>
        private SmtScope? _scope;

        /// <summary>
        /// Gets the current scope. Throws an error if no scopes have been created yet,
        /// as the parser should know when a scope has been created or not.
        /// </summary>
        public SmtScope Scope
        {
            get
            {
                if (_scope == null)
                {
                    throw new InvalidOperationException("Attempt to get a null scope.");
                }
                return _scope;
            }
        }

        /// <summary>
        /// Creates a new scope, with the current scope as its parent
        /// </summary>
        /// <returns>Context for ending the created scope</returns>
        public ISmtScopeProvider.ISmtScopeContext CreateNewScope()
        {
            _scope = new SmtScope(_scope);
            return this;
        }

        /// <summary>
        /// Ends the current scope
        /// </summary>
        public void Dispose()
        {
            _scope = Scope.Parent;
        }
    }
}
