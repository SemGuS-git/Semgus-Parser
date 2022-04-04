using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Parser.Reader;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// Command for declaring a new term type.
    /// Syntax: (declare-term-type [typename])
    /// </summary>
    internal class DeclareTermTypeCommand
    {
        private readonly ISemgusProblemHandler _handler;
        private readonly ISmtContextProvider _context;
        private readonly ISmtConverter _converter;
        private readonly ISourceMap _sourceMap;
        private readonly ILogger<DeclareTermTypeCommand> _logger;

        public DeclareTermTypeCommand(ISemgusProblemHandler handler, ISmtContextProvider context, ISmtConverter converter, ISourceMap sourceMap, ILogger<DeclareTermTypeCommand> logger)
        {
            _handler = handler;
            _context = context;
            _converter = converter;
            _sourceMap = sourceMap;
            _logger = logger;
        }

        [Command("declare-term-types")]
        public void DeclareTermType(IList<SortDecl> sortDecls, IList<IList<ConstructorDecl>> constructors)
        {
            using var loggerScope = _logger.BeginScope("While parsing `declare-term-types` command:");

            if (sortDecls.Count != constructors.Count)
            {
                throw _logger.LogParseErrorAndThrow("Term names and constructor lists must be matched.", _sourceMap[sortDecls]);
            }

            List<SemgusTermType> termTypes = new();
            foreach (var decl in sortDecls)
            {
                if (decl.Arity != 0)
                {
                    throw _logger.LogParseErrorAndThrow("Only arity 0 term types allowed.", _sourceMap[decl]);
                }
                SemgusTermType tt = new(decl.Identifier);
                _context.Context.AddSortDeclaration(tt);
                termTypes.Add(tt);
            }

            for (int ix = 0; ix < termTypes.Count; ++ix)
            {
                using var loggerTermTypeScope = _logger.BeginScope($"for term type '{termTypes[ix].Name}':");

                foreach (var constructor in constructors[ix])
                {
                    using var loggerConstructorScope = _logger.BeginScope($"in constructor '{constructor.Constructor}':");

                    List<SmtSort> children = new();
                    foreach (var child in constructor.Children)
                    {
                        if (_context.Context.TryGetSortDeclaration(child.Sort, out SmtSort? sort)) {
                            children.Add(sort);
                        }
                        else
                        {
                            throw _logger.LogParseErrorAndThrow("Sort not declared: " + child.Sort, _sourceMap[child.Sort]);
                        }
                    }
                    termTypes[ix].AddConstructor(new(constructor.Constructor, children.ToArray()));
                }
            }

            _handler.OnTermTypes(termTypes);
        }

        public record SortDecl(SmtSortIdentifier Identifier, int Arity) { }
        public record ConstructorDecl(SmtIdentifier Constructor, [Rest] IList<(SmtIdentifier Selector, SmtSortIdentifier Sort)> Children) { }
    }
}
