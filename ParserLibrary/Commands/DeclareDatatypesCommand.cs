using Microsoft.Extensions.Logging;

using Semgus.Model.Smt;
using Semgus.Model.Smt.Sorts;
using Semgus.Parser.Reader;

using System.Collections.Generic;
using System.Linq;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// Command class for (declare-sort symbol arity).
    /// Only exists to throw an error, as uninterpreted sorts are not supported.
    /// </summary>
    internal class DeclareDatatypesCommand
    {
        /// <summary>
        /// The problem handler
        /// </summary>
        private readonly ISemgusProblemHandler _handler;

        private readonly ISmtContextProvider _smtCtxProvider;

        private readonly ISourceContextProvider _srcCtxProvider;

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
        public DeclareDatatypesCommand(ISemgusProblemHandler hander, ISmtContextProvider smtCtxProvider, ISmtConverter converter, ISourceContextProvider srcCtxProvider, ISourceMap sourceMap, ILogger<DefineSortCommand> logger)
        {
            _handler = hander;
            _smtCtxProvider = smtCtxProvider;
            _converter = converter;
            _srcCtxProvider = srcCtxProvider;
            _sourceMap = sourceMap;
            _logger = logger;
        }

        [Command("declare-datatype")]
        public void DeclareDatatype()
        {

        }

        /// <summary>
        /// Declares an uninterpreted sort. Or it would, if it was supported.
        /// </summary>
        /// <param name="sortId">Symbol to name new sort</param>
        /// <param name="number">Arity of new sort</param>
        [Command("declare-datatypes")]
        public void DeclareDatatypes(SemgusToken declarations, SemgusToken definitions)
        {
            var ctx = _smtCtxProvider.Context;

            // Parse declarations and create initial datatype implementations
            if (!_converter.TryConvert(declarations, out IList<DatatypeDeclaration>? decls))
            {
                throw _logger.LogParseErrorAndThrow("Unable to parse datatype definitions. Expected list of identifiers and arities, but got: " + declarations, _sourceMap[declarations]);
            }

            List<SmtDatatype> datatypes = new();
            foreach (var decl in decls)
            {
                if (decl.Arity > 0)
                {
                    throw _logger.LogParseErrorAndThrow("Parameterized datatypes not yet supported.", _sourceMap[decl.Name]);
                }
                var dt = new SmtDatatype(new(decl.Name));
                datatypes.Add(dt);
                ctx.AddSortDeclaration(dt);
            }

            // Parse definitions
            if (!_converter.TryConvert(definitions, out IList<SemgusToken>? defnList))
            {
                throw _logger.LogParseErrorAndThrow("Expected list of datatype declarations, but got: " + definitions, _sourceMap[definitions]);
            }

            if (decls.Count != defnList.Count)
            {
                throw _logger.LogParseErrorAndThrow($"Mismatch in number of datatype declarations and definitions. Got {decls.Count} declarations and {defnList.Count} definitions.", _sourceMap[decls]);
            }

            for (int i = 0; i < defnList.Count; ++i)
            {
                if (!_converter.TryConvert(defnList[i], out IList<DatatypeConstructorDefinition>? constructors))
                {
                    throw _logger.LogParseErrorAndThrow("Unable to parse constructor list for datatype: " + decls[i].Name, _sourceMap[defnList[i]]);
                }

                foreach (var constructor in constructors)
                {
                    var childSorts = constructor.Children
                        .Select(c => c.Sort)
                        .Select(sid => ctx.GetSortOrDie(sid, _sourceMap, _logger));

                    SmtDatatypeConstructor cons = new(constructor.Name, datatypes[i], childSorts, _srcCtxProvider.CurrentSmtSource);
                    datatypes[i].AddConstructor(cons);
                    ctx.AddFunctionDeclaration(cons);
                }
                datatypes[i].Freeze();
            }

            _handler.OnDatatypes(ctx, datatypes);
        }

        private record DatatypeDeclaration(SmtIdentifier Name, int Arity);
        private record DatatypeConstructorDefinition(SmtIdentifier Name, [Rest] IList<(SmtIdentifier Selector, SmtSortIdentifier Sort)> Children);
    }
}
