using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Reader
{
    internal interface IExtensionHandler
    {
        public void ProcessExtensions(ISemgusProblemHandler handler, SmtContext ctx, SmtTerm term);
    }
}
