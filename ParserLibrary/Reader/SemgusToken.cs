using System;
using System.Collections;
using System.Linq;
using System.Text;

using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// Base class of tokens in the Semgus specification format
    /// </summary>
    public abstract record SemgusToken(SexprPosition? Position)
    {
        public abstract TResult Accept<TResult>(ISemgusTokenVisitor<TResult> visitor);

        public virtual bool IsLiteral => false;
    }

    /// <summary>
    /// Tokens with special meaning to the parser. Not to appear in input files
    /// </summary>
    public record SentinelToken(string Identifier) : SemgusToken(SexprPosition.Default)
    {
        public override TResult Accept<TResult>(ISemgusTokenVisitor<TResult> visitor) => visitor.VisitSentinel(this);
    }

    /// <summary>
    /// A symbol token
    /// </summary>
    public record SymbolToken(string Name, SexprPosition? Position) : SemgusToken(Position)
    {
        public override TResult Accept<TResult>(ISemgusTokenVisitor<TResult> visitor) => visitor.VisitSymbol(this);

        // Technically, there's more than this we need to check, but this is good enough for now
        public bool IsSmtLibCompliant => !char.IsDigit(Name[0]);

        public bool IsSmtLibInternal => Name[0] == '@' || Name[0] == '.';

        public bool IsSmtLibReserved => _smtLibReserved.Contains(Name);

        public override string ToString() => Name;

        private static readonly string[] _smtLibReserved =
        {
            "BINARY", "DECIMAL", "HEXADECIMAL", "NUMERAL", "STRING",
            "_", "!",
            "as", "let", "exists", "forall", "match", "par"
        };
    }

    /// <summary>
    /// A keyword token. Essentially just a specialized type of symbol.
    /// Note that the name does not include the leading colon.
    /// </summary>
    public record KeywordToken(string Name, SexprPosition? Position) : SemgusToken(Position)
    {
        public override TResult Accept<TResult>(ISemgusTokenVisitor<TResult> visitor) => visitor.VisitKeyword(this);

        public override string ToString() => $":{Name}";
    }

    /// <summary>
    /// A literal string token. Does not include the surrounding quotation marks or escaped quotes.
    /// Note that this is a "raw" string, in that it may contain any literal character. Any standard
    /// escape sequences present in the source (e.g., \n) are not unescaped. The only preprocessing
    /// done is to translate "" --> ".
    /// </summary>
    public record StringToken(string Value, SexprPosition? Position) : SemgusToken(Position)
    {
        public override TResult Accept<TResult>(ISemgusTokenVisitor<TResult> visitor) => visitor.VisitString(this);

        public override bool IsLiteral => true;
    }

    /// <summary>
    /// A literal natural number (including 0)
    /// </summary>
    public record NumeralToken(long Value, SexprPosition? Position) : SemgusToken(Position)
    {
        public override TResult Accept<TResult>(ISemgusTokenVisitor<TResult> visitor) => visitor.VisitNumeral(this);

        public override bool IsLiteral => true;
    }

    /// <summary>
    /// A literal real number (encoded as a double-precision binary float)
    /// </summary>
    public record DecimalToken(double Value, SexprPosition? Position) : SemgusToken(Position)
    {
        public override TResult Accept<TResult>(ISemgusTokenVisitor<TResult> visitor) => visitor.VisitDecimal(this);

        public override bool IsLiteral => true;
    }

    /// <summary>
    /// A literal bit vector
    /// </summary>
    public record BitVectorToken(BitArray Value, SexprPosition? Position) : SemgusToken(Position)
    {
        public override TResult Accept<TResult>(ISemgusTokenVisitor<TResult> visitor) => visitor.VisitBitVector(this);

        public override bool IsLiteral => true;
    }

    public interface IConsOrNil
    {
        public bool IsNil();
        public SemgusToken First();
        public IConsOrNil? Rest();
        public SexprPosition? Position { get; }
    }

    /// <summary>
    /// Nil, a.k.a. the empty list ().
    /// Note that nicely mapping this to C# types is difficult; in reality, Nil should be a 
    /// sub-type of everything else (including cons), but since multiple inheritance isn't 
    /// supported, we just shoe-horn it in as an independent token type.
    /// </summary>
    public record NilToken(SexprPosition? Position) : SemgusToken(Position), IConsOrNil
    {
        public override TResult Accept<TResult>(ISemgusTokenVisitor<TResult> visitor) => visitor.VisitNil(this);

        public override string ToString() => "nil";

        public bool IsNil() => true;
        public SemgusToken First() => this;
        public IConsOrNil? Rest() => this;
    }

    /// <summary>
    /// A cons cell, used as the building blocks of lists.
    /// Why have a discrete cons type and not a list, when we don't expect to have 
    /// conses not as a part of lists? Sometimes we need to pass the tail of a list to
    /// a function, but C# doesn't have a built-in list type that lets you do this.
    /// So we go the traditional route and build our own linked lists with conses.
    /// </summary>
    public record ConsToken(SemgusToken Head, SemgusToken Tail, SexprPosition? Position) : SemgusToken(Position), IConsOrNil
    {
        public override TResult Accept<TResult>(ISemgusTokenVisitor<TResult> visitor) => visitor.VisitCons(this);

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append('(');
            ToStringInner(builder);
            builder.Append(')');
            return builder.ToString();
        }

        private void ToStringInner(StringBuilder builder)
        {
            builder.Append(Head.ToString());

            if (Tail is ConsToken cons)
            {
                builder.Append(' ');
                cons.ToStringInner(builder);
            }
            else if (Tail is not NilToken)
            {
                builder.Append(" . ");
                builder.Append(Tail.ToString());
            }
        }

        public bool IsNil() => false;
        public SemgusToken First() => Head;
        public IConsOrNil? Rest() => Tail as IConsOrNil;
    }
}
