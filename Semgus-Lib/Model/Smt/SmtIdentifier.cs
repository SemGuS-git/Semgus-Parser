using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    public sealed record SmtIdentifier(string Symbol, params SmtIdentifier.Index[] Indices)
    {
        public sealed class Index
        {
            public Index(string symbol)
            {
                StringValue = symbol;
            }

            public Index(long numeral)
            {
                NumeralValue = numeral;
                StringValue = numeral.ToString();
            }

            public readonly string StringValue;
            public readonly long? NumeralValue;

            public override int GetHashCode()
            {
                return StringValue.GetHashCode() + (NumeralValue.HasValue ? 1 : 0);
            }

            public override bool Equals(object? obj)
            {
                return obj is not null &&
                    obj is Index i &&
                    i.StringValue == this.StringValue &&
                    i.NumeralValue == this.NumeralValue;
            }
        }

        public bool Equals(SmtIdentifier? other)
        {
            if (other == null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Indices.Length == other.Indices.Length
                && Symbol == other.Symbol
                && Indices.SequenceEqual(other.Indices);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Symbol.GetHashCode();
                foreach (Index i in Indices)
                {
                    hash = hash * 23 + i.GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString()
        {
            if (Indices.Length == 0)
            {
                return Symbol;
            }
            else
            {
                return $"(_ {Symbol} {string.Join(' ', Indices.Select(i => i.StringValue))})";
            }
        }
    }
}
