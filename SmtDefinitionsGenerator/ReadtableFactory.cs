using Semgus.Sexpr.Reader;

using System;
using System.Collections.Generic;
using System.Text;

namespace Semgus.SmtDefinitionsGenerator
{
    public class ReadtableFactory
    {
        public static Readtable<object> CreateReadtable()
        {
            var readtable = Readtable<object>.GetDefaultReadtable();
            readtable.Case = ReadtableCase.Preserve;

            // Configure dispatching macros
            readtable.SetDispatchMacroCharacter('#', 'x', ReaderHooks.HexLiteralHook);
            readtable.SetDispatchMacroCharacter('#', 'b', ReaderHooks.BitVectorLiteralHook);
            readtable.SetDispatchMacroCharacter('#', '|', ReaderHooks.BlockCommentHook);

            return readtable;
        }
    }
}
