using System;
using System.Collections;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Semgus.Parser.Internal;
using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    /// <summary>
    /// Visitor that converts literal CST nodes into corresponding generic-typed literal AST nodes.
    /// </summary>
    public class LiteralConverter : ISemgusTokenVisitor<LiteralBase>
    {
        public static LiteralConverter Default { get; } = new();

        public LiteralBase VisitSentinel(SentinelToken sentinel)
        {
            throw new InvalidOperationException("Sentinels are not literals: " + sentinel.Identifier);
        }

        public LiteralBase VisitSymbol(SymbolToken symbol)
        {
            throw new InvalidOperationException("Symbols are not literals: " + symbol.Name);
        }

        public LiteralBase VisitKeyword(KeywordToken keyword)
        {
            throw new InvalidOperationException("Keywords are not literals: " + keyword.Name);
        }

        public LiteralBase VisitString(StringToken str)
        {
            return new Literal<string>(value: str.Value);
        }

        public LiteralBase VisitNumeral(NumeralToken num)
        {
            // TODO: int vs. long
            return new Literal<int>(value: Convert.ToInt32(num.Value));
        }

        public LiteralBase VisitDecimal(DecimalToken dec)
        {
            return new Literal<double>(value: dec.Value);
        }

        public LiteralBase VisitBitVector(BitVectorToken bv)
        {
            return new Literal<BitArray>(bv.Value);
        }

        public LiteralBase VisitNil(NilToken nil)
        {
            // This one is interesting...A nil token is really just an empty list, so...
            throw new InvalidOperationException("Nil is not approriate in a literal context: " + nil);
        }

        public LiteralBase VisitCons(ConsToken cons)
        {
            throw new InvalidOperationException("Conses are not literals: " + cons.ToString());
        }
    }

}