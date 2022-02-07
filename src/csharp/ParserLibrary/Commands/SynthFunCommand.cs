using Semgus.Model.Smt;
using Semgus.Parser.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Parser.Commands
{
    internal class SynthFunCommand
    {
        private readonly ISemgusProblemHandler _handler;
        private readonly ISmtContextProvider _context;

        public SynthFunCommand(ISemgusProblemHandler handler, ISmtContextProvider context)
        {
            _handler = handler;
            _context = context;
        }

        [Command("synth-fun")]
        public void SynthFun(SmtIdentifier name, IList<SmtConstant> args, SmtSort ret /* No grammar declaration yet */)
        {
            _handler.OnSynthFun(_context.Context, name, args, ret);
        }
    }
}
