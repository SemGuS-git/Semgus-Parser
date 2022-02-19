using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    public sealed record SmtIdentifier(string Symbol, params string[] Indices)
    {
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
                foreach (string s in Indices)
                {
                    hash = hash * 23 + s.GetHashCode();
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
                return $"(_ {Symbol} ...todo...)";
            }
        }
    }
}
