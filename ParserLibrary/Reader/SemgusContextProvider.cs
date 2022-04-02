using Semgus.Model;

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// Holds a reference to the current SemGuS context
    /// </summary>
    public interface ISemgusContextProvider
    {
        /// <summary>
        /// The current SemGuS context
        /// </summary>
        public SemgusContext Context { get; set; }
    }

    /// <summary>
    /// Holds a reference to the current SemGuS context
    /// </summary>
    internal class SemgusContextProvider : ISemgusContextProvider
    {
        /// <summary>
        /// The current SemGuS context
        /// </summary>
        public SemgusContext Context { get; set; }
    }
}
