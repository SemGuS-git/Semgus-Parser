using Semgus.Parser.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Commands
{
    internal class CheckSynthCommand
    {
        private readonly ISemgusProblemHandler _handler;
        private readonly ISmtContextProvider _smtCtxProvider;
        private readonly ISemgusContextProvider _semgusCtxProvider;

        public CheckSynthCommand(ISemgusProblemHandler handler, ISmtContextProvider smtCtxProvider, ISemgusContextProvider semgusCtxProvider)
        {
            _handler = handler;
            _smtCtxProvider = smtCtxProvider;
            _semgusCtxProvider = semgusCtxProvider;
        }

        [Command("check-synth")]
        public void CheckSynth()
        {
            _handler.OnCheckSynth(_smtCtxProvider.Context, _semgusCtxProvider.Context);
        }
    }
}
