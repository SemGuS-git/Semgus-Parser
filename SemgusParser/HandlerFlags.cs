using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser
{
    /// <summary>
    /// Flags for handlers
    /// </summary>
    internal class HandlerFlags
    {
        /// <summary>
        /// Creatse a new HandlerFlags instance for the given context and flag factory
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fac"></param>
        private HandlerFlags(InvocationContext ctx, HandlerFlagsFactory fac)
        {
            _ctx = ctx;
            _fac = fac;
        }

        /// <summary>
        /// Context for these flags
        /// </summary>
        private readonly InvocationContext _ctx;

        /// <summary>
        /// Factory that created these flags
        /// </summary>
        private readonly HandlerFlagsFactory _fac;

        /// <summary>
        /// Whether or not to generate function events (JSON only)
        /// </summary>
        public bool FunctionEvents => _fac.FunctionEvents.GetFlag(_ctx);

        /// <summary>
        /// Class for configuring and creating handler flags
        /// </summary>
        public class HandlerFlagsFactory
        {
            /// <summary>
            /// Whether or not to generate function events (JSON only)
            /// </summary>
            public readonly CommandFlag FunctionEvents = new("function-events", isHidden: true);

            /// <summary>
            /// Creates a new HandlerFlagsFactory and configures it for the given command
            /// </summary>
            /// <param name="command">Command to configure flags for</param>
            public HandlerFlagsFactory(Command command)
            {
                command.AddFlag(FunctionEvents);
            }

            /// <summary>
            /// Gets a HandlerFlags instance for the given context
            /// </summary>
            /// <param name="ic">Context for flags</param>
            /// <returns>HandlerFlags for the given context</returns>
            public HandlerFlags GetFlags(InvocationContext ic)
            {
                return new HandlerFlags(ic, this);
            }
        }
    }
}
