using Microsoft.Extensions.Logging;

using Semgus.Model.Smt;
using Semgus.Parser.Reader;

using System.Collections.Generic;
using System.Linq;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// Command class for (declare-sort symbol arity).
    /// Only exists to throw an error, as uninterpreted sorts are not supported.
    /// </summary>
    internal class DefineSortCommand
    {
        /// <summary>
        /// The problem handler
        /// </summary>
        private readonly ISemgusProblemHandler _handler;

        private readonly ISmtContextProvider _smtCtxProvider;

        /// <summary>
        /// Converter for converting with
        /// </summary>
        private readonly ISmtConverter _converter;

        /// <summary>
        /// The source map
        /// </summary>
        private readonly ISourceMap _sourceMap;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<DefineSortCommand> _logger;

        /// <summary>
        /// Creates a DeclareSortCommand with the given dependencies
        /// </summary>
        /// <param name="hander">The problem handler</param>
        /// <param name="sourceMap">The source map</param>
        /// <param name="logger">The logger</param>
        public DefineSortCommand(ISemgusProblemHandler hander, ISmtContextProvider smtCtxProvider, ISmtConverter converter, ISourceMap sourceMap, ILogger<DefineSortCommand> logger)
        {
            _handler = hander;
            _smtCtxProvider = smtCtxProvider;
            _converter = converter;
            _sourceMap = sourceMap;
            _logger = logger;
        }

        /// <summary>
        /// Declares an uninterpreted sort. Or it would, if it was supported.
        /// </summary>
        /// <param name="sortId">Symbol to name new sort</param>
        /// <param name="number">Arity of new sort</param>
        [Command("define-sort")]
        public void DefineSort(SymbolToken sortSymbol, IList<SymbolToken> sortParams, SmtSortIdentifier targetSort)
        {
            var ctx = _smtCtxProvider.Context;

            // Resolve sort parameter symbols to actual sort identifiers
            SmtSortIdentifier[] paramIds = sortParams.Select(p => new SmtSortIdentifier(p.Name)).ToArray();

            // Check: make sure all parameters of targetSort are actual sorts or our sort parameters
            foreach (var targetParam in targetSort.Parameters)
            {
                if (paramIds.Contains(targetParam))
                {
                    continue;
                }

                //ctx.TryGetSortDeclaration()
            }
            throw _logger.LogParseErrorAndThrow("Defining sort aliases not supported yet.", _sourceMap[sortSymbol]);
        }
    }
}
