using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt;

namespace Semgus.Parser.Reader
{
    public interface ISmtContextProvider
    {
        public SmtContext Context { get; set; }
    }

    internal class SmtContextProvider : ISmtContextProvider
    {
        public SmtContext Context { get; set; }
    }
}
