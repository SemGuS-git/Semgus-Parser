using Semgus.Model.Smt;

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// Holds a reference to the current SMT context
    /// </summary>
    public interface ISmtContextProvider
    {
        /// <summary>
        /// The current SMT context
        /// </summary>
        public SmtContext Context { get; set; }
    }

    /// <summary>
    /// Holds a reference to the current SMT context
    /// </summary>
    internal class SmtContextProvider : ISmtContextProvider
    {
        /// <summary>
        /// The current SMT context
        /// </summary>
        public SmtContext Context { get; set; } = null!;
    }
}
