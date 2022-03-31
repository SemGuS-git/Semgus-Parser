using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt.Terms;

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// Term used by the parser to indicate that an error happened at a lower level.
    /// This lets errors propagate upward during term parsing, so we can try and get
    /// more information parsed out of the problem instead of failing fast.
    /// </summary>
    internal class ErrorTerm : SmtTerm
    {
        /// <summary>
        /// Why this term is in error
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Constructs a new error term with the given message
        /// </summary>
        /// <param name="message">Why this term is in error</param>
        public ErrorTerm(string message) : base(ErrorSort.Instance)
        {
            Message = message;
        }

        /// <summary>
        /// Accepts a visitor for this term. Always throws an exception.
        /// </summary>
        /// <typeparam name="TOutput">Visitor output type</typeparam>
        /// <param name="visitor">Visitor to accept</param>
        /// <returns>Does not return</returns>
        /// <exception cref="InvalidOperationException">Always thrown, as visiting an error term is an error.</exception>
        public override TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor)
        {
            throw new InvalidOperationException("Visiting an error term.");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "<error>";
        }
    }
}
