using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Transforms
{
    public class SmtMacroExpander : SmtTermWalker<object?>
    {
        public SmtMacroExpander() : base(default(object))
        {
        }

        public override (SmtTerm, object?) OnFunctionApplication(SmtFunctionApplication appl, IReadOnlyList<SmtTerm> arguments, IReadOnlyList<object?> up)
        {
            var applicable = appl.Definition;
            if (applicable is SmtMacro macro)
            {
                return (MacroExpand(macro, appl.Rank, appl.Arguments), up);
            }
            else
            {
                return base.OnFunctionApplication(appl, arguments, up);
            }
        }

        private SmtTerm MacroExpand(SmtMacro macro, SmtFunctionRank rank, IReadOnlyList<SmtTerm> args)
        {
            return null!;
        }
    }
}
