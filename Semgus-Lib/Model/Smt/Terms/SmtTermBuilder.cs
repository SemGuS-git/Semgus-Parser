using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    /// <summary>
    /// Methods for programmatically building SMT terms
    /// </summary>
    public class SmtTermBuilder
    {
        /// <summary>
        /// The default error reporter. Just throws an exception.
        /// </summary>
        private static readonly Action<object, string> _defaultErrorHandler 
            = (thing, msg) => throw new InvalidOperationException(msg);

        /// <summary>
        /// The current context for this builder
        /// </summary>
        private readonly SmtContext _ctx;

        /// <summary>
        /// The current error reporter
        /// </summary>
        private readonly Action<object, string> _onError = _defaultErrorHandler;

        /// <summary>
        /// Creates a new SmtTermBuilder for the given context
        /// </summary>
        /// <param name="ctx"></param>
        public SmtTermBuilder(SmtContext ctx)
        {
            _ctx = ctx;
        }

        /// <summary>
        /// Creates a function application term
        /// </summary>
        /// <param name="id">Function to apply</param>
        /// <param name="args">Arguments to application</param>
        /// <returns>Application term</returns>
        /// <exception cref="InvalidOperationException">On unhandled errors</exception>
        public SmtTerm Apply(SmtIdentifier id, params SmtTerm[] args)
            => Apply(_ctx, _onError, id, args);

        /// <summary>
        /// Creates a function application term
        /// </summary>
        /// <param name="ctx">Context for this term</param>
        /// <param name="id">Function to apply</param>
        /// <param name="args">Arguments to application</param>
        /// <returns>Application term</returns>
        /// <exception cref="InvalidOperationException">On unhandled errors</exception>
        public static SmtTerm Apply(SmtContext ctx, SmtIdentifier id, params SmtTerm[] args)
            => Apply(ctx, _defaultErrorHandler, id, args);

        /// <summary>
        /// Creates a function application term
        /// </summary>
        /// <param name="ctx">Context for this term</param>
        /// <param name="onError">Action for error reporting</param>
        /// <param name="id">Function to apply</param>
        /// <param name="args">Arguments to application</param>
        /// <returns>Application term</returns>
        /// <exception cref="InvalidOperationException">On unhandled errors</exception>
        public static SmtTerm Apply(SmtContext ctx, Action<object, string> onError, SmtIdentifier id, params SmtTerm[] args)
        {
            if (!ctx.TryGetFunctionDeclaration(id, out SmtFunction? function))
            {
                onError(id, $"Unable to resolve function: {id}");
                throw new InvalidOperationException();
            }

            if (!function.TryResolveRank(out SmtFunctionRank? rank, default, args.Select(a => a.Sort).ToArray()))
            {
                onError(id, $"Unable to resolve function rank: {id}, with arg sorts {args.Select(a => a.Sort).ToArray()}");
                throw new InvalidOperationException();
            }

            return new SmtFunctionApplication(function, rank, args.ToList());
        }
    }
}
