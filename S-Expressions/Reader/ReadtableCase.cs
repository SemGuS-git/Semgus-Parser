
namespace Semgus.Sexpr.Reader
{
    /// <summary>
    /// Reader modes for dealing with casing
    /// </summary>
    public enum ReadtableCase
    {
        /// <summary>
        /// Upcasing mode - all symbol names are converted to upper case when read.
        /// This is the default for Common Lisp formats
        /// </summary>
        Upcase,

        /// <summary>
        /// Case-preserving mode - symbols are read as-is, with no case conversions.
        /// This is the default for SMT-LIB2 formats
        /// </summary>
        Preserve,
    }
}
