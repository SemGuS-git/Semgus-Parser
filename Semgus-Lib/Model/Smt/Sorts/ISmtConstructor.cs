using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Sorts
{
    /// <summary>
    /// Common interface for "constructor"-like things
    /// </summary>
    public interface ISmtConstructor
    {
        /// <summary>
        /// Constructor name
        /// </summary>
        public SmtIdentifier Name { get; }

        /// <summary>
        /// Constructor child sorts
        /// </summary>
        public IReadOnlyList<SmtSort> Children { get; }
    }
}
