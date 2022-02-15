using Microsoft.Extensions.Logging;

using Semgus.Model;
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
        private readonly ILogger<SynthFunCommand> _logger;

        public SynthFunCommand(ISemgusProblemHandler handler, ISmtContextProvider context, ILogger<SynthFunCommand> logger)
        {
            _handler = handler;
            _context = context;
            _logger = logger;
        }

        [Command("synth-fun")]
        public void SynthFun(SmtIdentifier name, IList<SmtConstant> args, SmtSort ret /* No grammar declaration yet */)
        {
            // Currently, only Semgus-style synth-funs are supported
            if (args.Count > 0 || ret is not TermType tt)
            {
                _logger.LogParseError("Only Semgus-style `synth-fun`s are supported, with no arguments and a term type as the return.", default);
                throw new InvalidOperationException("Only Semgus-style `synth-fun`s are supported, with no arguments and a term type as the return.");
            }

            var rank = new SmtFunctionRank(tt);
            var decl = new SmtFunction(name, SmtTheory.UserDefined, rank);
            _context.Context.AddFunctionDeclaration(decl);

            _handler.OnSynthFun(_context.Context, name, args, ret);
        }
    }
}
