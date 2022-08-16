using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// Handler for extension functions
    /// </summary>
    internal interface IExtensionHandler
    {
        /// <summary>
        /// Processes extensions in an SMT term and emits function definitions as necessary
        /// </summary>
        /// <param name="handler">Problem handler to emit definitions to</param>
        /// <param name="ctx">SMT context</param>
        /// <param name="term">Term to process</param>
        public void ProcessExtensions(ISemgusProblemHandler handler, SmtContext ctx, SmtTerm term);
    }
}
