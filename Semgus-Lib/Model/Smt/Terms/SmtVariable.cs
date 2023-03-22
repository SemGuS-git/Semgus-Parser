using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    /// <summary>
    /// A variable in an SMT expression
    /// </summary>
    public class SmtVariable : SmtTerm
    {
        /// <summary>
        /// Constructs a new SMT variable
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <param name="binding">The binding that establishes this variable</param>
        public SmtVariable(SmtIdentifier name, SmtVariableBinding binding) : base(binding.Sort)
        {
            Name = name;
            Binding = binding;
        }

        /// <summary>
        /// Name of this variable
        /// </summary>
        public SmtIdentifier Name { get; }

        /// <summary>
        /// Binding for this variable
        /// </summary>
        public SmtVariableBinding Binding { get; }

        /// <summary>
        /// Accepts a term visitor and dispatches for this variable
        /// </summary>
        /// <typeparam name="TOutput">Output type of the visitor</typeparam>
        /// <param name="visitor">The visitor</param>
        /// <returns>Result of visiting this variable</returns>
        public override TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor)
        {
            return visitor.VisitVariable(this);
        }

        /// <summary>
        /// Gets this variable as a string
        /// </summary>
        /// <returns>The variable name as a string</returns>
        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
