using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt;

namespace Semgus.Parser.Reader
{
    public interface ISmtScopeProvider
    {
        public SmtScope Scope { get; }

        public ISmtScopeContext CreateNewScope();

        public interface ISmtScopeContext : ISmtScopeProvider, IDisposable
        {
        }
    }

    internal class SmtScopeProvider : ISmtScopeProvider, ISmtScopeProvider.ISmtScopeContext
    {
        private SmtScope? _scope;
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

        public ISmtScopeProvider.ISmtScopeContext CreateNewScope()
        {
            _scope = new SmtScope(_scope);
            return this;
        }

        public void Dispose()
        {
            _scope = Scope.Parent;
        }
    }
}
