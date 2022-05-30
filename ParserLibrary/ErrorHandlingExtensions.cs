using Microsoft.Extensions.Logging;

using Semgus.Model.Smt;
using Semgus.Parser.Reader;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser
{
    internal static class ErrorHandlingExtensions
    {
        /// <summary>
        /// Attempts to resolve a sort with the given name, or throws a parse exception if not able
        /// </summary>
        /// <param name="ctx">This SmtContext</param>
        /// <param name="id">The sort identifier to resolve</param>
        /// <param name="sourceMap">Source map for error message generation</param>
        /// <param name="logger">Logger to log with</param>
        /// <returns>The resolved sort</returns>
        /// <exception>FatalParseException when unable to resolve sort</exception>
        [return: NotNullIfNotNull("id")]
        public static SmtSort? GetSortOrDie<T>(this SmtContext ctx, SmtSortIdentifier? id, ISourceMap sourceMap, ILogger<T> logger)
        {
            if (id is null)
            {
                return null;
            }

            if (!ctx.TryGetSortDeclaration(id, out SmtSort? sort))
            {
                throw logger.LogParseErrorAndThrow($"Undeclared sort: {id}", sourceMap[id]);
            }
            else
            {
                return sort;
            }
        }

        /// <summary>
        /// Attempts to resolve a function with the given name, or throws a parse exception if not able
        /// </summary>
        /// <param name="ctx">This SmtContext</param>
        /// <param name="id">The function identifier to resolve</param>
        /// <param name="sourceMap">Source map for error message generation</param>
        /// <param name="logger">Logger to log with</param>
        /// <returns>The resolved function</returns>
        /// <exception>FatalParseException when unable to resolve function</exception>
        [return: NotNullIfNotNull("id")]
        public static SmtFunction? GetFunctionOrDie<T>(this SmtContext ctx, SmtIdentifier? id, ISourceMap sourceMap, ILogger<T> logger)
        {
            if (id is null)
            {
                return null;
            }

            if (!ctx.TryGetFunctionDeclaration(id, out SmtFunction? function))
            {
                throw logger.LogParseErrorAndThrow($"Undeclared function: {id}", sourceMap[id]);
            }
            else
            {
                return function;
            }
        }
    }
}
