
namespace Semgus.Sexpr.Reader
{
    /// <summary>
    /// A position in some source associated with an S-expression
    /// </summary>
    /// <param name="Line">The line number</param>
    /// <param name="Column">The column number</param>
    /// <param name="Source">The source of this position. Could be a filename or other identifier</param>
    public record SexprPosition(int Line, int Column, string Source)
    {
        /// <summary>
        /// Returns a new position pointing to the next column
        /// </summary>
        /// <returns>Position pointing to the next column</returns>
        public SexprPosition NextColumn()
        {
            return new SexprPosition(Line, Column + 1, Source);
        }

        /// <summary>
        /// Returns a new position pointing to the start of the next line
        /// </summary>
        /// <returns>Position pointing to the next line</returns>
        public SexprPosition NextLine()
        {
            return new SexprPosition(Line + 1, 1, Source);
        }

        /// <summary>
        /// The default position, pointed at 0:0 with an unknown source
        /// </summary>
        public static readonly SexprPosition Default = new(0, 0, "<unknown>");

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Source}:{Line}:{Column}";
        }
    }
}
