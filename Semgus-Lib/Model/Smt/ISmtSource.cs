namespace Semgus.Model.Smt
{
    /// <summary>
    /// A source for an SMT object, e.g., theories or logics
    /// </summary>
    public interface ISmtSource
    {
        /// <summary>
        /// The name of this source
        /// </summary>
        SmtIdentifier Name { get; }
    }
}
