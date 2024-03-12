using Microsoft.Extensions.Logging;

using Semgus.Model.Smt;
using Semgus.Parser.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// Intrinsic commands. These are an extension to declare "intrinsic" functions,
    /// constants, and sorts. These only are added to the SMT context for parsing, but
    /// are not emitted as a part of the problem file. They are assumed to be handled by
    /// another tool down the line, for example, theory extensions in an SMT solver.
    /// </summary>
    internal class IntrinsicCommands
    {
        private readonly ISmtContextProvider _smtCtxProvider;
        private readonly ISourceMap _sourceMap;
        private readonly ISourceContextProvider _sourceContextProvider;
        private readonly ILogger<IntrinsicCommands> _logger;

        /// <summary>
        /// Creates a new IntrinsicCommands instance
        /// </summary>
        /// <param name="smtCtxProvider">The SMT context provider</param>
        /// <param name="sourceMap">The source map</param>
        /// <param name="sourceContextProvider">The source context provider</param>
        /// <param name="handler">Not used, but required for DI</param>
        /// <param name="logger">The logger</param>
        public IntrinsicCommands(ISmtContextProvider smtCtxProvider,
                                 ISourceMap sourceMap,
                                 ISourceContextProvider sourceContextProvider,
                                 ISemgusProblemHandler handler,
                                 ILogger<IntrinsicCommands> logger)
        {
            _smtCtxProvider = smtCtxProvider;
            _sourceMap = sourceMap;
            _sourceContextProvider = sourceContextProvider;
            _logger = logger;
        }

        /// <summary>
        /// Declares an "intrinsic" constant
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="argIds">Function argument sorts</param>
        /// <param name="returnSortId">Function return sort</param>
        [Command("declare-intrinsic-fun")]
        public void DeclareIntrinsicFun(SmtIdentifier name, IList<SmtSortIdentifier> argIds, SmtSortIdentifier returnSortId)
        {
            using var logScope = _logger.BeginScope($"while processing `declare-intrinsic-fun` for {name}:");

            var returnSort = _smtCtxProvider.Context.GetSortOrDie(returnSortId, _sourceMap, _logger);
            var args = argIds.Select(argId => _smtCtxProvider.Context.GetSortOrDie(argId, _sourceMap, _logger));
            var rank = new SmtFunctionRank(returnSort, args.ToArray());
            var decl = new SmtFunction(name, _sourceContextProvider.CurrentSmtSource, rank);

            _smtCtxProvider.Context.AddFunctionDeclaration(decl);
        }

        /// <summary>
        /// Declares an "intrinsic" constant
        /// </summary>
        /// <param name="name">Constant name</param>
        /// <param name="returnSortId">Sort of constant</param>
        [Command("declare-intrinsic-const")]
        public void DeclareIntrinsicConst(SmtIdentifier name, SmtSortIdentifier returnSortId)
        {
            using var logScope = _logger.BeginScope($"while processing `declare-intrinsic-const` for {name}:");

            var returnSort = _smtCtxProvider.Context.GetSortOrDie(returnSortId, _sourceMap, _logger);
            var rank = new SmtFunctionRank(returnSort, Array.Empty<SmtSort>());
            var decl = new SmtFunction(name, _sourceContextProvider.CurrentSmtSource, rank);

            _smtCtxProvider.Context.AddFunctionDeclaration(decl);
        }

        /// <summary>
        /// Declares an "intrinsic" sort
        /// </summary>
        /// <param name="name">Sort name to declare</param>
        [Command("declare-intrinsic-sort")]
        public void DeclareIntrinsicSort(SmtSortIdentifier name)
        {
            using var logScope = _logger.BeginScope($"while processing `declare-intrinsic-sort` for {name}:");

            var sort = new SmtSort.GenericSort(name);

            _smtCtxProvider.Context.AddSortDeclaration(sort);
        }
    }
}
