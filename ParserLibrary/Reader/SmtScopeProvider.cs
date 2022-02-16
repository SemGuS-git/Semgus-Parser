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
        public SmtScope Scope { get; private set; }

        public ISmtScopeProvider.ISmtScopeContext CreateNewScope()
        {
            Scope = new SmtScope(Scope);
            return this;
        }

        public void Dispose()
        {
            Scope = Scope.Parent;
        }
    }
}
