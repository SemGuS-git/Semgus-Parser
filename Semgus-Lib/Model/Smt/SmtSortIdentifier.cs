using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// Symbolic representation of a sort
    /// </summary>
    /// <param name="Name">The name of this sort</param>
    /// <param name="Parameters">Sort parameters for this sort</param>
    public sealed record SmtSortIdentifier(SmtIdentifier Name, params SmtSortIdentifier[] Parameters)
    {
        /// <summary>
        /// Creates a new simple SmtSortIdentifier with the given name (as a string)
        /// </summary>
        /// <param name="name">The string name</param>
        public SmtSortIdentifier(string name) : this(new SmtIdentifier(name))
        {
        }

        /// <summary>
        /// Gets the number of parameters this sort uses
        /// </summary>
        public int Arity => Parameters.Length;

        /// <summary>
        /// Checks if this sort identifier represents the same sort as the other 
        /// </summary>
        /// <param name="other">Sort identifier to check against</param>
        /// <returns>True if the same sort</returns>
        public bool Equals(SmtSortIdentifier? other)
        {
            if (other == null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Parameters.Length == other.Parameters.Length
                && Name == other.Name
                && Parameters.SequenceEqual(other.Parameters);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Name.GetHashCode();
                foreach (SmtSortIdentifier s in Parameters)
                {
                    hash = hash * 23 + s.GetHashCode();
                }
                return hash;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Parameters.Length == 0)
            {
                return Name.ToString();
            }
            else
            {
                return $"({Name} {string.Join(' ', Parameters.Select(p => p.ToString()))})";
            }
        }
    }
}
