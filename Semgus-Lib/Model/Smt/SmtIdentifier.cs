using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// An SMT identifier, possibly indexed
    /// </summary>
    /// <param name="Symbol">The primary symbol for this identifier</param>
    /// <param name="Indices">Indices for this identifier</param>
    public sealed record SmtIdentifier(string Symbol, params SmtIdentifier.Index[] Indices)
    {
        /// <summary>
        /// An index for an identifier
        /// </summary>
        public sealed class Index
        {
            /// <summary>
            /// Creates a new string-valued index
            /// </summary>
            /// <param name="symbol">The string index</param>
            public Index(string symbol)
            {
                StringValue = symbol;
            }

            /// <summary>
            /// Creates a new numeral-valued index
            /// </summary>
            /// <param name="numeral">The numeral index</param>
            public Index(long numeral)
            {
                NumeralValue = numeral;
                StringValue = numeral.ToString();
            }

            /// <summary>
            /// The string value of this index
            /// </summary>
            public readonly string StringValue;

            /// <summary>
            /// The numeral value of this index, if this is a numeral-valued index
            /// </summary>
            public readonly long? NumeralValue;

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                return StringValue.GetHashCode() + (NumeralValue.HasValue ? 1 : 0);
            }

            /// <inheritdoc/>
            public override bool Equals(object? obj)
            {
                return obj is not null &&
                    obj is Index i &&
                    i.StringValue == this.StringValue &&
                    i.NumeralValue == this.NumeralValue;
            }

            /// <summary>
            /// Converts a string into an index
            /// </summary>
            /// <param name="symbol">String to index on</param>
            public static implicit operator Index(string symbol) => new Index(symbol);

            /// <summary>
            /// Converts a long into an index
            /// </summary>
            /// <param name="numeral">Long to index on</param>
            public static implicit operator Index(long numeral) => new Index(numeral);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
