using Microsoft.Extensions.Logging;

using Semgus.Model.Smt;
using Semgus.Parser.Reader;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// Command class for (declare-sort symbol arity).
    /// Only exists to throw an error, as uninterpreted sorts are not supported.
    /// </summary>
    internal class DeclareSortCommand
    {
        /// <summary>
        /// The problem handler
        /// </summary>
        private readonly ISemgusProblemHandler _handler;

        /// <summary>
        /// The source map
        /// </summary>
        private readonly ISourceMap _sourceMap;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<DeclareSortCommand> _logger;

        /// <summary>
        /// Creates a DeclareSortCommand with the given dependencies
        /// </summary>
        /// <param name="hander">The problem handler</param>
        /// <param name="sourceMap">The source map</param>
        /// <param name="logger">The logger</param>
        public DeclareSortCommand(ISemgusProblemHandler hander, ISourceMap sourceMap, ILogger<DeclareSortCommand> logger)
        {
            _handler = hander;
            _sourceMap = sourceMap;
            _logger = logger;
        }

        /// <summary>
        /// Declares an uninterpreted sort. Or it would, if it was supported.
        /// </summary>
        /// <param name="sortId">Symbol to name new sort</param>
        /// <param name="number">Arity of new sort</param>
        [Command("declare-sort")]
        public void DeclareSort(SmtIdentifier sortId, NumeralToken number)
        {
            throw _logger.LogParseErrorAndThrow("Declaring uninterpreted sorts is unsupported. File an issue if you have a real use case for these.", _sourceMap[sortId]);
        }
    }
}
