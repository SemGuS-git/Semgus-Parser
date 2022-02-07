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
    internal class DefineFunsRecCommand
    {
        [Command("define-funs-rec")]
        public static void DefineFunsRec(IList<Signature> signatures, IList<Definitions> definitions)
        {
            // Implementation here
        }

        public record Signature(SmtIdentifier Name, IList<(SmtIdentifier, SmtSort)> args, SmtSort ret);

        public record Definitions(/* etc. */);
    }
}
