using System.Collections.Generic;
using System.IO;

using Semgus.Parser.Reader;
using Semgus.Syntax;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// A command that can appear at the top-level of a SemGuS file
    /// </summary>
    public interface ISemgusCommand
    {
        /// <summary>
        /// Command name
        /// </summary>
        string CommandName { get; }

        /// <summary>
        /// Hook for processing a command invocation
        /// </summary>
        /// <param name="previous">The SemgusProblem, prior to invocation of this command</param>
        /// <param name="commandForm">The command form</param>
        /// <returns>The new SemgusProblem after invoking this command</returns>
        SemgusProblem Process(SemgusProblem previous, ConsToken commandForm, TextWriter errorStream, ref int errCount);
    }

    /// <summary>
    /// Extensions for Semgus commands
    /// </summary>
    public static class SemgusCommandExtensions
    {
        /// <summary>
        /// Returns this command and name wrapped in a key-value pair
        /// </summary>
        /// <returns>A key-value pair wrapping this command</returns>
        public static KeyValuePair<string, ISemgusCommand> AsKeyValuePair(this ISemgusCommand command)
        {
            return KeyValuePair.Create(command.CommandName, command);
        }
    }
}
