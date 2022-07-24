using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Transforms
{
    public class SmtMacroExpander : SmtTermWalker<object?>
    {
        private readonly SmtContext _context;

        public SmtMacroExpander(SmtContext ctx) : base(default(object))
        {
            _context = ctx;
        }

        public SmtTerm Expand(SmtTerm term)
        {
            return (term.Accept(this)).Item1;
        }

        public static SmtTerm Expand(SmtContext context, SmtTerm term)
            => new SmtMacroExpander(context).Expand(term);

        public override (SmtTerm, object?) VisitFunctionApplication(SmtFunctionApplication functionApplication)
        {
            if (functionApplication.Definition is SmtMacro macro
                && macro.ShouldExpand(_context, functionApplication.Rank))
            {
                if (TryMacroExpand(functionApplication, out var expansion))
                {
                    return expansion.Accept(this);
                }
            }
            return base.VisitFunctionApplication(functionApplication);
        }

        public bool TryMacroExpand(SmtTerm toExpand, out SmtTerm expansion)
        {
            bool didExpand;
            bool didExpandAny = false;
            do
            {
                didExpand = TryMacroExpand1(toExpand, out expansion);
                didExpandAny |= didExpand;
                toExpand = expansion;
            }
            while (didExpand);
            return didExpandAny;
        }

        public bool TryMacroExpand1(SmtTerm toExpand, out SmtTerm expansion)
        {
            if (toExpand is SmtFunctionApplication appl
                && appl.Definition is SmtMacro macro
                && macro.ShouldExpand(_context, appl.Rank))
            {
                expansion = macro.DoExpand(_context, appl.Arguments);
                return expansion != toExpand;
            }
            else
            {
                expansion = toExpand;
                return false;
            }
        }
    }
}
