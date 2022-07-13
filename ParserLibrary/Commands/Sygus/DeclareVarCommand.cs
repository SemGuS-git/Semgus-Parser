using Microsoft.Extensions.Logging;

using Semgus.Model.Smt;
using Semgus.Parser.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Commands.Sygus
{
    internal class DeclareVarCommand
    {
        private readonly ISemgusContextProvider _semContextProvider;
        private readonly ISmtContextProvider _smtContextProvider;
        private readonly ISourceMap _sourceMap;
        private readonly ILogger<DeclareVarCommand> _logger;

        public DeclareVarCommand(ISemgusProblemHandler _,
                                 ISemgusContextProvider semContextProvider,
                                 ISmtContextProvider smtContextProvider,
                                 ISourceMap sourceMap,
                                 ILogger<DeclareVarCommand> logger)
        {
            _semContextProvider = semContextProvider;
            _smtContextProvider = smtContextProvider;
            _sourceMap = sourceMap;
            _logger = logger;
        }

        [Command("declare-var")]
        public void SetLogic(SmtIdentifier name, SmtSortIdentifier sortId)
        {
            _semContextProvider.Context.AddSygusVar(name, _smtContextProvider.Context.GetSortOrDie(sortId, _sourceMap, _logger));
        }
    }
}
