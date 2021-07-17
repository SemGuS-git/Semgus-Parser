using System.IO;

using Semgus.Parser.Reader;
using Semgus.Syntax;

namespace Semgus.Parser.Commands
{
    /// <summary>
    /// Command to add arbitrary metadata to a Semgus problem
    /// </summary>
    public class MetadataCommand : ISemgusCommand
    {
        /// <summary>
        /// This command's name
        /// </summary>
        public string CommandName => "metadata";

        /// <summary>
        /// Parses out the metadata command. This consists of a keyword followed by some arbitrary S-expression.
        /// For now, it's just ignored.
        /// </summary>
        /// <param name="previous">The state of the SemgusProblem before this command</param>
        /// <param name="commandForm">The S-expression form for this command invocation</param>
        /// <param name="errorStream">Stream to write errors to</param>
        /// <param name="errCount">Number of encountered errors</param>
        /// <returns>The state of the SemgusProblem after this command</returns>
        public SemgusProblem Process(SemgusProblem previous, ConsToken commandForm, TextWriter errorStream, ref int errCount)
        {
            return previous;
        }
    }
}
