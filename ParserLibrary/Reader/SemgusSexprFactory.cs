using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Reader
{
    public class SemgusSexprFactory : ISexprFactory<SemgusToken>
    {
        public SemgusToken ConstructCons(SemgusToken head, SemgusToken tail, SexprPosition? position = null)
        {
            return new ConsToken(head, tail, position);
        }

        public SemgusToken ConstructDecimal(double value, SexprPosition? position = null)
        {
            return new DecimalToken(value, position);
        }

        public SemgusToken ConstructKeyword(ReadOnlySpan<char> keyword, SexprPosition? position = null)
        {
            string name;
            if (':' == keyword[0])
            {
                name = keyword[1..].ToString();
            }
            else
            {
                name = keyword.ToString();
            }
            return new KeywordToken(name, position);
        }

        public SemgusToken ConstructList(IList<SemgusToken> list, SexprPosition? position = null)
        {
            if (0 == list.Count)
            {
                return ConstructNil(position);
            }
            else
            {
                SemgusToken tail = ConstructNil();
                for (int ix = list.Count - 1; ix >= 0; --ix)
                {
                    // Note: we want the FIRST cons to be at the given position,
                    //       but each inner cons just starts where its head does.
                    tail = ConstructCons(list[ix], tail, 0 == ix ? position : list[ix].Position);
                }
                return tail;
            }
        }

        public SemgusToken ConstructNil(SexprPosition? position = null)
        {
            return new NilToken(position);
        }

        public SemgusToken ConstructNumeral(long value, SexprPosition? position = null)
        {
            return new NumeralToken(value, position);
        }

        public SemgusToken ConstructSentinel(string identifier)
        {
            return new SentinelToken(identifier);
        }

        public SemgusToken ConstructString(ReadOnlySpan<char> str, SexprPosition? position = null)
        {
            return new StringToken(str.ToString(), position);
        }

        public SemgusToken ConstructSymbol(ReadOnlySpan<char> name, SexprPosition? position = null)
        {
            return new SymbolToken(name.ToString(), position);
        }

        public SemgusToken ConstructBitVector(BitArray bv, SexprPosition? position = null)
        {
            return new BitVectorToken(bv, position);
        }
    }
}
