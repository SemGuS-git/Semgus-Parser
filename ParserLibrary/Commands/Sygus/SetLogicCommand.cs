using Semgus.Model.Smt;
using Semgus.Parser.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Commands.Sygus
{
    internal class SetLogicCommand
    {
        [Command("set-logic")]
        public static void SetLogic(SmtIdentifier logic)
        {
            // no-op
        }
    }
}
