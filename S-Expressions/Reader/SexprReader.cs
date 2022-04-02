using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Semgus.Sexpr.Reader
{
    /// <summary>
    /// A reader that turns a stream of text into S-expressions
    /// </summary>
    /// <typeparam name="TSexprRoot"></typeparam>
    public class SexprReader<TSexprRoot>
    {
        /// <summary>
        /// The stream of text
        /// </summary>
        private readonly TextReader _reader;
        
        /// <summary>
        /// The readtable to use
        /// </summary>
        private readonly Readtable<TSexprRoot> _readtable;

        /// <summary>
        /// Factory for constructing S-expressions from syntactic elements
        /// </summary>
        private readonly ISexprFactory<TSexprRoot> _sexprFactory;

        /// <summary>
        /// The S-expression factory used by this reader
        /// </summary>
        public ISexprFactory<TSexprRoot> SexprFactory => _sexprFactory;

        /// <summary>
        /// A special S-expression denoting the end of the file being read
        /// </summary>
        public TSexprRoot EndOfFileSentinel { get; }

        /// <summary>
        /// A special S-expression denoting that no S-expression was read
        /// </summary>
        public TSexprRoot NothingSentinel { get; }

        /// <summary>
        /// The current position in the data. Points to the character that was most recently read
        /// </summary>
        public SexprPosition CurrentPosition { get; private set; } = new SexprPosition(0, 0, "<unknown>");

        /// <summary>
        /// The name of the source used to report positions
        /// </summary>
        public string SourceName
        {
            get => CurrentPosition.Source;
            set => CurrentPosition = new SexprPosition(CurrentPosition.Line, CurrentPosition.Column, value);
        }

        /// <summary>
        /// Constructs a new reader
        /// </summary>
        /// <param name="sexprFactory">Factory for creating S-expressions</param>
        /// <param name="readtable">Readtable to use</param>
        /// <param name="reader">TextReader for reading from</param>
        public SexprReader(ISexprFactory<TSexprRoot> sexprFactory, Readtable<TSexprRoot> readtable, TextReader reader)
        {
            _readtable = readtable ?? Readtable<TSexprRoot>.GetDefaultReadtable();
            _sexprFactory = sexprFactory;
            _reader = reader;
            EndOfFileSentinel = _sexprFactory.ConstructSentinel("%reader-eof");
            NothingSentinel = _sexprFactory.ConstructSentinel("%no-read-object");
        }

        /// <summary>
        /// Constructs a new reader from a stream
        /// </summary>
        /// <param name="sexprFactory">Factory for creating S-expressions</param>
        /// <param name="readtable">Readtable to use</param>
        /// <param name="stream">Stream to read from</param>
        public SexprReader(ISexprFactory<TSexprRoot> sexprFactory, Readtable<TSexprRoot> readtable, Stream stream)
            : this(sexprFactory, readtable, new StreamReader(stream))
        {
        }

        /// <summary>
        /// Constructs a new reader from a string
        /// </summary>
        /// <param name="sexprFactory">Factory for creating S-expressions</param>
        /// <param name="readtable">Readtable to use</param>
        /// <param name="stream">String to read from</param>
        public SexprReader(ISexprFactory<TSexprRoot> sexprFactory, Readtable<TSexprRoot> readtable, string str)
            : this(sexprFactory, readtable, new StringReader(str))
        {
        }

        /// <summary>
        /// Stores the previous character read, for purposes of line and column tracking.
        /// Starts out as \n, so the first character read is on a fresh line (1:1)
        /// </summary>
        private char previous = '\n';

        /// <summary>
        /// Updates the current position count based on the previously and currently read characters
        /// </summary>
        /// <param name="c">The currently read character</param>
        private void UpdateCurrentPosition(char c)
        {
            // Handle \n and \r\n (as we skip incrementing the line for \r\n on the \r
            if ('\n' == previous)
            {
                CurrentPosition = CurrentPosition.NextLine();
            }
            // Handle just \r (not followed by \n)
            else if ('\r' == previous && '\n' != c)
            {
                CurrentPosition = CurrentPosition.NextLine();
            }
            else
            {
                CurrentPosition = CurrentPosition.NextColumn();
            }
            previous = c;
        }

        /// <summary>
        /// Tries to get the next character from the stream
        /// </summary>
        /// <param name="c">The read character</param>
        /// <param name="syntaxType">The syntax type of the read character</param>
        /// <returns>True if successfully read, false if at end of stream</returns>
        public bool TryReadCharacter(out char c, out SyntaxType syntaxType)
        {
            int read;
            if (_peekedCharacter >= 0)
            {
                read = _peekedCharacter;
                _peekedCharacter = -1;
            }
            else
            {
                read = _reader.Read();
            }
            if (-1 == read)
            {
                c = default;
                syntaxType = SyntaxType.Invalid;
                return false;
            }
            else
            {
                c = (char)read;
                UpdateCurrentPosition(c);
                syntaxType = _readtable.GetSyntaxFromChar(c);
                return true;
            }
        }

        /// <summary>
        /// If >= 0, we "peeked" this character (but actually read it)
        /// </summary>
        private int _peekedCharacter = -1;

        /// <summary>
        /// Tries to get the next character from the stream, without advancing
        /// </summary>
        /// <param name="c">The peeked character</param>
        /// <param name="syntaxType">The syntax type of the peeked character</param>
        /// <returns>True if successfully peeked, false if at end of stream</returns>
        public bool TryPeekCharacter(out char c, out SyntaxType syntaxType)
        {
            int read;
            if (_peekedCharacter >= 0)
            {
                // Double peek. Return what we already have.
                read = _peekedCharacter;
            }
            else
            {
                read = _reader.Read();
                _peekedCharacter = read;
            }
            if (-1 == read)
            {
                c = default;
                syntaxType = SyntaxType.Invalid;
                return false;
            }
            else
            {
                c = (char)read;
                syntaxType = _readtable.GetSyntaxFromChar(c);
                return true;
            }
        }

        /// <summary>
        /// Reads an S-expression from the stream
        /// </summary>
        /// <param name="errorOnEndOfStream">Whether or not to throw an exception on end-of-stream. Defaults to true</param>
        /// <returns>The read S-expression, or the EndOfFileSentinel if at EOF and errorOnEndOfStream is false</returns>
        public TSexprRoot Read(bool errorOnEndOfStream = true)
        {
            while (true)
            {
                TSexprRoot ret = ReadImpl();
                if (ret!.Equals(NothingSentinel))
                {
                    continue;
                }
                else if (EndOfFileSentinel!.Equals(ret))
                {
                    if (errorOnEndOfStream)
                    {
                        throw new EndOfStreamException("End of stream while reading");
                    }
                    else
                    {
                        return EndOfFileSentinel;
                    }
                }
                else
                {
                    return ret;
                }
            }
        }

        /// <summary>
        /// Attempts to read an S-expression from the stream
        /// </summary>
        /// <returns>The read expression, or NothingSentinel if nothing was read (e.g., a comment)</returns>
        private TSexprRoot ReadImpl()
        {
            ConsumeWhitespace();

            if (!TryPeekCharacter(out char c, out var syntaxType))
            {
                return EndOfFileSentinel;
            }

            // Invalid character - reject
            if (SyntaxType.Invalid == syntaxType)
            {
                TryReadCharacter(out _, out _);
                throw new Exception("Found invalid character: " + c); // TODO - more specific exception. Should try and read it actually, too
            }

            // Macro character - dispatch and return
            if (SyntaxType.Macro == syntaxType)
            {
                TryReadCharacter(out _, out _);
                return _readtable.GetMacroCharacter(c)(this, c, CurrentPosition);
            }

            // Save off the current position to use when processing constituents
            // Note this is plus one column because we only peeked at the first character
            SexprPosition position = CurrentPosition.NextColumn();

            // Otherwise, read constituents until the next character of another syntax type
            string constituents = ConsumeConstituents();

            return ProcessConstituents(constituents, position);
        }

        /// <summary>
        /// Processes a span of constituent characters, and converts it to the appropriate atom
        /// </summary>
        /// <param name="constituents">Span of constituent characters</param>
        /// <param name="position">Position of the first character in the span</param>
        /// <param name="allAlphabetic">Whether or not to treat all characters as alphabetic (i.e., escaped)</param>
        /// <returns>The read atom</returns>
        public TSexprRoot ProcessConstituents(ReadOnlySpan<char> constituents, SexprPosition? position = null, bool allAlphabetic = false)
        {
            ConstituentTrait getCT(char c)
            {
                if (allAlphabetic)
                {
                    return ConstituentTrait.Alphabetic;
                }
                else
                {
                    return _readtable.GetConstituentTrait(c);
                }
            }

            var original = constituents;
            bool isKeyword = false;
            // We still want to ban numbers with leading zeros, lest they be confused with octal
            bool maybeNumberWithLeadingZero = false;

            // This is messy because it's adapted from SBCL's reader.lisp
            // Please excuse the use of goto
            char first = constituents[0];
            constituents = constituents[1..];

            var firstCT = getCT(first);
            if (firstCT.HasFlag(ConstituentTrait.Zero)) goto zero;
            if (firstCT.HasFlag(ConstituentTrait.Digit)) goto digit;
            if (firstCT.HasFlag(ConstituentTrait.PackageMarker)) goto keyword;
            if (firstCT.HasFlag(ConstituentTrait.Alphabetic)) goto symbol;
            throw new Exception("Invalid character: '" + first + "' in: " + original.ToString());

        zero:
            if (constituents.IsEmpty)
            {
                return _sexprFactory.ConstructNumeral(0, position);
            }
            else if (getCT(constituents[0]).HasFlag(ConstituentTrait.DecimalPoint))
            {
                goto digit;
            }
            else if (getCT(constituents[0]).HasFlag(ConstituentTrait.Digit))
            {
                maybeNumberWithLeadingZero = true;
                goto digit;
            }
            else
            {
                goto symbol;
            }

        digit:
            {
                bool isDecimal = false;
                foreach (char c in constituents)
                {
                    var ct = getCT(c);
                    if (ct.HasFlag(ConstituentTrait.DecimalPoint))
                    {
                        if (isDecimal)
                        {
                            // Extra dot. Not a number.
                            goto symbol;
                        }
                        else
                        {
                            isDecimal = true;
                        }
                    }
                    else if (!ct.HasFlag(ConstituentTrait.Digit))
                    {
                        // Not a number. Must be a symbol.
                        goto symbol;
                    }
                }

                if (maybeNumberWithLeadingZero)
                {
                    throw new InvalidOperationException("Numbers with leading zeros are forbidden: " + original.ToString());
                }

                if (isDecimal)
                {
                    return _sexprFactory.ConstructDecimal(double.Parse(original), position);
                }
                else
                {
                    return _sexprFactory.ConstructNumeral(long.Parse(original), position);
                }
            }

        keyword:
            if (constituents.IsEmpty)
            {
                throw new Exception("Empty keyword symbol: " + original.ToString());
            }
            isKeyword = true;
            goto symbol;
        
        symbol:
            foreach (char c in constituents)
            {
                if (!getCT(c).HasFlag(ConstituentTrait.Alphabetic))
                {
                    throw new Exception("Invalid character in symbol: '" + c + "' in symbol: " + original.ToString());
                }
            }

            string thing = isKeyword ? constituents.ToString() : original.ToString();

            if (!allAlphabetic) /* a.k.a. escaped */
            {
                switch (_readtable.Case)
                {
                    case ReadtableCase.Preserve: break;
                    case ReadtableCase.Upcase: thing = thing.ToUpper(); break;
                    default: throw new InvalidOperationException("Unknown readtable case mode: " + _readtable.Case);
                }
            }

            if (isKeyword)
            {
                return _sexprFactory.ConstructKeyword(thing, position);
            }
            else
            {
                return _sexprFactory.ConstructSymbol(thing, position);
            }
        }

        /// <summary>
        /// Consumes constituent characters from the stream until a non-constituent is encountered
        /// </summary>
        /// <returns>The read characters</returns>
        public string ConsumeConstituents()
        {
            StringBuilder builder = new();
            while (true)
            {
                if (!TryPeekCharacter(out _, out var syntaxType))
                {
                    return builder.ToString();
                }

                if (SyntaxType.Constituent != syntaxType)
                {
                    return builder.ToString();
                }

                TryReadCharacter(out char c, out _);
                builder.Append(c);
            }
        }

        /// <summary>
        /// Consumes whitespace characters from the stream until a non-whitespace character is encountered
        /// </summary>
        public void ConsumeWhitespace()
        {
            while (true)
            {
                if (!TryPeekCharacter(out _, out var syntaxType))
                {
                    return; // End of stream is end of whitespace, too
                }

                if (SyntaxType.Whitespace != syntaxType)
                {
                    return;
                }

                TryReadCharacter(out _, out _);
            }
        }

        /// <summary>
        /// Reads a list of S-expressions until encountering the given delimiter
        /// </summary>
        /// <param name="delimiter">Delimiter to stop reading when hit</param>
        /// <param name="l">The list of S-expressions read</param>
        /// <returns>True if successfully read the list, false if end-of-stream encountered</returns>
        public bool TryReadDelimitedList(char delimiter, [NotNullWhen(true)] out IList<TSexprRoot>? l)
        {
            List<TSexprRoot> list = new();
            while (true)
            {
                ConsumeWhitespace();
                if (!TryPeekCharacter(out char c, out _))
                {
                    l = default;
                    return false;
                }

                if (delimiter == c)
                {
                    TryReadCharacter(out _, out _);
                    l = list;
                    return true;
                }
                else
                {
                    var sexpr = ReadImpl();
                    if (sexpr!.Equals(NothingSentinel) || sexpr!.Equals(EndOfFileSentinel))
                    {
                        continue;
                    }
                    else
                    {
                        list.Add(sexpr);
                    }
                }
            }
        }
    }
}
