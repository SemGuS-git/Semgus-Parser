﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt;
using Semgus.Parser.Reader;

namespace Semgus.Parser.Commands
{
    internal class SetInfoCommand
    {
        private readonly ISemgusProblemHandler _handler;
        private readonly ISmtContextProvider _context;

        public SetInfoCommand(ISemgusProblemHandler hander, ISmtContextProvider ctx)
        {
            _handler = hander;
            _context = ctx;
        }

        [Command("set-info")]
        public void SetInfo([Rest] IList<SmtAttribute> attrs)
        {
            foreach (var attr in attrs)
            {
                _handler.OnSetInfo(_context.Context, attr);
            }
        }
    }
}
