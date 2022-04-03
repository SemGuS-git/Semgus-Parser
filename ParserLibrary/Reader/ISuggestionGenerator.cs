using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// Generates suggestions for identifiers based on context
    /// </summary>
    internal interface ISuggestionGenerator
    {
        /// <summary>
        /// Get suggestions for functions similar to the given identifier
        /// </summary>
        /// <param name="id">Seed identifier</param>
        /// <param name="context">Current SMT context</param>
        /// <param name="arity">Arity of function to suggest</param>
        /// <returns>Enumeration of suggested function identifiers</returns>
        IEnumerable<SmtIdentifier> GetFunctionSuggestions(SmtIdentifier id, SmtContext context, int arity);
        
        /// <summary>
        /// Get suggestions for variables or constants similar to the given identifier
        /// </summary>
        /// <param name="id">Seed identifier</param>
        /// <param name="context">Current SMT context</param>
        /// <param name="scope">Current SMT scope</param>
        /// <returns>Enumeration of suggested variable or constant identifiers</returns>
        IEnumerable<SmtIdentifier> GetVariableSuggestions(SmtIdentifier id, SmtContext context, SmtScope scope);
    }
}
