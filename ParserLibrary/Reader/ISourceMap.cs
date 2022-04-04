using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// Stores a mapping between objects and their position in an input stream
    /// </summary>
    internal interface ISourceMap
    {
        /// <summary>
        /// The position for a given object
        /// </summary>
        /// <param name="key">Object for source mapping</param>
        /// <returns>Position of the object</returns>
        SexprPosition this[object key] { get; set; }
    }
}
