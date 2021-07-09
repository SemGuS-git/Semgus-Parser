
namespace Semgus.Sexpr.Reader
{
    /// <summary>
    /// Different syntax types of characters. See § 2.1.4 of the CLHS for details.
    /// Note that the single-escape '\' is not permitted in SMT-LIB syntax, and we
    /// are able to treat '|' as a macro character because it is only allowed at the
    /// beginning of a symbol and not in the middle. Also note that '#' is not a 
    /// non-terminating macro character in SMT-LIB syntax.
    /// 
    /// Another difference is that simple symbols can only contain a certain set of
    /// characters (not all constituents), but that is handled when interning symbols.
    /// Constituents, likewise, are only printable characters, not control characters.
    /// </summary>
    public enum SyntaxType
    {
        /// <summary>
        /// Not a valid character that can appear in an S-expression
        /// </summary>
        Invalid,

        /// <summary>
        /// A character that is part of an identifier, number, or other standard construct
        /// </summary>
        Constituent,

        /// <summary>
        /// A character that triggers a reader macro when read
        /// </summary>
        Macro,

        /// <summary>
        /// A character that is treated as a delimiter between constituents
        /// </summary>
        Whitespace
    }
}
