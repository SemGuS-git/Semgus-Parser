using Semgus.Sexpr.Reader;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Semgus.SmtDefinitionsGenerator
{
    internal class TokenFactory : ISexprFactory<object>
    {
        public object ConstructBitVector(BitArray bv, SexprPosition position = null)
            => bv;

        public object ConstructCons(object head, object tail, SexprPosition position = null)
        {
            throw new InvalidOperationException("Loose conses not allowed.");
        }

        public object ConstructDecimal(double value, SexprPosition position = null)
            => value;

        public object ConstructKeyword(ReadOnlySpan<char> keyword, SexprPosition position = null)
            => new KeywordToken(keyword.ToString());

        public object ConstructList(IList<object> list, SexprPosition position = null)
            => list;

        public object ConstructNil(SexprPosition position = null)
            => null;

        public object ConstructNumeral(long value, SexprPosition position = null)
            => value;

        public object ConstructSentinel(string identifier)
            => new SentinelToken(identifier.ToString());

        public object ConstructString(ReadOnlySpan<char> str, SexprPosition position = null)
            => str.ToString();

        public object ConstructSymbol(ReadOnlySpan<char> name, SexprPosition position = null)
            => new SymbolToken(name.ToString());
    }
}
