using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Reader
{
    public interface ISemgusTokenVisitor<TResult>
    {
        TResult VisitSentinel(SentinelToken sentinel);

        TResult VisitSymbol(SymbolToken symbol);

        TResult VisitKeyword(KeywordToken keyword);

        TResult VisitString(StringToken str);

        TResult VisitNumeral(NumeralToken num);

        TResult VisitDecimal(DecimalToken dec);

        TResult VisitBitVector(BitVectorToken bv);

        TResult VisitNil(NilToken nil);

        TResult VisitCons(ConsToken cons);
    }
}
