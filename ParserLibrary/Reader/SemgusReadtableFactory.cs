using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Reader
{
    public class SemgusReadtableFactory
    {
        public static Readtable<SemgusToken> CreateSemgusReadtable()
        {
            var readtable = Readtable<SemgusToken>.GetDefaultReadtable();
            readtable.Case = ReadtableCase.Preserve;

            // Configure dispatching macros
            readtable.SetDispatchMacroCharacter('#', 'x', ReaderHooks.HexLiteralHook);
            readtable.SetDispatchMacroCharacter('#', 'b', ReaderHooks.BitVectorLiteralHook);
            readtable.SetDispatchMacroCharacter('#', '|', ReaderHooks.BlockCommentHook);

            return readtable;
        }
    }
}
