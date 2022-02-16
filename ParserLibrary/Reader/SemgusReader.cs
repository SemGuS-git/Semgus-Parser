using System.IO;

using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// A reader for S-expressions in SemGuS files. A convenience wrapper around a SexprReader over SemgusTokens.
    /// </summary>
    public class SemgusReader
    {
        /// <summary>
        /// The backing reader. We wrap the reader, instead of inheriting, to hide the complexity of the interface
        /// </summary>
        private readonly SexprReader<SemgusToken> _reader;

        /// <summary>
        /// Constructs a new SemgusReader over the given string
        /// </summary>
        /// <param name="str">The string to read from</param>
        public SemgusReader(string str)
        {
            _reader = new(new SemgusSexprFactory(), SemgusReadtableFactory.CreateSemgusReadtable(), str);
        }

        /// <summary>
        /// Constructs a new SemgusReader over the given stream
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        public SemgusReader(Stream stream)
        {
            _reader = new(new SemgusSexprFactory(), SemgusReadtableFactory.CreateSemgusReadtable(), stream);
        }

        /// <summary>
        /// Updates the source name associated with reported positions
        /// </summary>
        /// <param name="source">Source name to use</param>
        public void SetSourceName(string source)
        {
            _reader.SourceName = source;
        }

        /// <summary>
        /// Reads a SemgusToken from this reader
        /// </summary>
        /// <param name="errorOnEndOfStream">Whether or not to throw an error on EOF</param>
        /// <returns>The read token, or EndOfFileSentinel if at the end and errorOnEndOfStream is false</returns>
        public SemgusToken Read(bool errorOnEndOfStream = true)
        {
            return _reader.Read(errorOnEndOfStream);
        }

        /// <summary>
        /// Sentinel returned when at the end of the file
        /// </summary>
        public SemgusToken EndOfFileSentinel => _reader.EndOfFileSentinel;
    }
}
