using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace Semgus.Sexpr.Reader
{
    /// <summary>
    /// Delegate type for macro hooks
    /// </summary>
    /// <param name="reader">The current reader</param>
    /// <param name="character">The character this hook was dispatched from</param>
    /// <param name="position">The reader position on the macro character</param>
    /// <returns>The read s-expr, or null if nothing read</returns>
    public delegate TSexprRoot MacroHook<TSexprRoot>(SexprReader<TSexprRoot> reader, char character, SexprPosition? position);

    /// <summary>
    /// Delegate type for dispatching macros
    /// </summary>
    /// <param name="reader">The current reader</param>
    /// <param name="prefix">The prefix argument</param>
    /// <param name="character">The character this hook was dispatched from</param>
    /// <param name="position">The reader position on the dispatching character</param>
    /// <returns></returns>
    public delegate TSexprRoot DispatchMacroHook<TSexprRoot>(SexprReader<TSexprRoot> reader, int prefix, char character, SexprPosition? position);

    /// <summary>
    /// Hooks used in the default readtable
    /// </summary>
    public static class ReaderHooks
    {
        /// <summary>
        /// Hook for reading a list, ending at a right parenthesis
        /// </summary>
        /// <typeparam name="TSexprRoot">Type of s-expression to produce</typeparam>
        /// <param name="reader">The current reader</param>
        /// <param name="c">Character triggering this hook (not used)</param>
        /// <param name="pos">Position of character that triggered this hook</param>
        /// <returns>List that is read</returns>
        public static TSexprRoot LParenHook<TSexprRoot>(SexprReader<TSexprRoot> reader, char c, SexprPosition? pos)
        {
            if (!reader.TryReadDelimitedList(')', out var list))
            {
                throw new Exception($"End of stream parsing form starting at: {pos}");
            }
            return reader.SexprFactory.ConstructList(list, pos);
        }

        /// <summary>
        /// Hook for reading a right parenthesis, not as part of a list
        /// </summary>
        /// <typeparam name="TSexprRoot">Type of s-expression to produce</typeparam>
        /// <param name="reader">The current reader</param>
        /// <param name="c">Character triggering this hook (not used)</param>
        /// <param name="pos">Position of character that triggered this hook</param>
        /// <returns>Never returns</returns>
        /// <exception cref="Exception">Unconditionally thrown, as reading an unmatched right parenthesis is an error</exception>
        public static TSexprRoot RParenHook<TSexprRoot>(SexprReader<TSexprRoot> reader, char c, SexprPosition? pos)
        {
            throw new Exception("Found unmatched " + c);
        }

        /// <summary>
        /// Hook for reading a line comment. Consumes the entire rest of the line
        /// </summary>
        /// <typeparam name="TSexprRoot">Type of s-expression to produce</typeparam>
        /// <param name="reader">The current reader</param>
        /// <param name="c">Character triggering this hook (not used)</param>
        /// <param name="pos">Position of character that triggered this hook</param>
        /// <returns>The sentinel for nothing read</returns>
        public static TSexprRoot CommentHook<TSexprRoot>(SexprReader<TSexprRoot> reader, char c, SexprPosition? pos)
        {
            while (true)
            {
                if (!reader.TryReadCharacter(out c, out _))
                {
                    return reader.EndOfFileSentinel;
                }

                if ('\n' == c || '\r' == c)
                {
                    break;
                }
            }
            return reader.NothingSentinel;
        }

        /// <summary>
        /// Hook for reading an escaped symbol
        /// </summary>
        /// <typeparam name="TSexprRoot">Type of s-expression to produce</typeparam>
        /// <param name="reader">The current reader</param>
        /// <param name="c">Character triggering this hook (not used)</param>
        /// <param name="pos">Position of character that triggered this hook</param>
        /// <returns>The escaped symbol</returns>
        public static TSexprRoot EscapeHook<TSexprRoot>(SexprReader<TSexprRoot> reader, char c, SexprPosition? pos)
        {
            StringBuilder builder = new();
            while (true)
            {
                if (!reader.TryReadCharacter(out c, out _))
                {
                    throw new Exception("Unmatched '|' at end of stream");
                }

                if ('|' == c)
                {
                    break;
                }
                else
                {
                    builder.Append(c);
                }
            }
            return reader.ProcessConstituents(builder.ToString(), pos, allAlphabetic: true);
        }

        /// <summary>
        /// Hook for reading a string. Only double quotes are escaped (doubled). Everything else is read raw
        /// </summary>
        /// <typeparam name="TSexprRoot">Type of s-expression to produce</typeparam>
        /// <param name="reader">The current reader</param>
        /// <param name="c">Character triggering this hook, as well as the terminator</param>
        /// <param name="pos">Position of character that triggered this hook</param>
        /// <returns>String (not including surrounding quotes) that is read</returns>
        public static TSexprRoot StringHook<TSexprRoot>(SexprReader<TSexprRoot> reader, char c, SexprPosition? pos)
        {
            StringBuilder builder = new();
            while (true)
            {
                if (!reader.TryReadCharacter(out char next, out _))
                {
                    throw new Exception("Unterminated string at end of stream");
                }

                if (c == next)
                {
                    if (!reader.TryPeekCharacter(out char peeked, out _) || c != peeked)
                    {
                        return reader.SexprFactory.ConstructString(builder.ToString(), pos);
                    }
                    else
                    {
                        reader.TryReadCharacter(out _, out _); // Consume escaped terminator
                    }
                }

                builder.Append(next);
            }
        }

        /// <summary>
        /// Dispatch hook for reading a block comment, possibly nested
        /// </summary>
        /// <typeparam name="TSexprRoot">Type of s-expression to produce</typeparam>
        /// <param name="reader">The current reader</param>
        /// <param name="prefix">Not used</param>
        /// <param name="c">The character triggering this hook, as well as the end delimiter</param>
        /// <param name="pos">Position of character that triggered this hook</param>
        /// <returns>The nothing sentinel</returns>
        public static TSexprRoot BlockCommentHook<TSexprRoot>(SexprReader<TSexprRoot> reader, int prefix, char c, SexprPosition? pos)
        {
            int nesting = 1;
            char last = default;

            while (true)
            {
                if (!reader.TryReadCharacter(out char next, out _))
                {
                    throw new Exception("EndOfStream parsing block comment");
                }

                // Comment end
                if ('#' == next && c == last)
                {
                    nesting -= 1;
                    if (0 == nesting)
                    {
                        // Done parsing this comment, but no s-expr actually read
                        return reader.NothingSentinel;
                    }
                }
                // Comment start - to support nesting 
                else if (c == next && '#' == last)
                {
                    nesting += 1;
                }

                last = next;
            }
        }

        /// <summary>
        /// Dispatch hook for reading a hexadecimal number
        /// </summary>
        /// <typeparam name="TSexprRoot">Type of s-expression to produce</typeparam>
        /// <param name="reader">The current reader</param>
        /// <param name="prefix">Not used</param>
        /// <param name="c">The character triggering this hook</param>
        /// <param name="pos">Position of character that triggered this hook</param>
        /// <returns>A numeral representing the encoded hexadecimal number</returns>
        public static TSexprRoot HexLiteralHook<TSexprRoot>(SexprReader<TSexprRoot> reader, int prefix, char c, SexprPosition? pos)
        {
            // We expect a sequence of hex digits. We read all remaining constituents and report errors if not the case
            string hex = reader.ConsumeConstituents();

            BitArray bits = new BitArray(hex.Length * 4);
            foreach (char nibble in hex)
            {
                if (!byte.TryParse(nibble.ToString(), System.Globalization.NumberStyles.HexNumber, default, out byte value))
                {
                    throw new InvalidOperationException("Expected hexadecimal number, but got: #x" + hex);
                }
                for (int i = 0; i < 4; ++i)
                {
                    bits.LeftShift(1);
                    bits.Set(0, (value & 0b1000) > 0);
                    value <<= 1;
                }
            }
            return reader.SexprFactory.ConstructBitVector(bits, pos);
        }

        /// <summary>
        /// Dispatch hook for reading a bit vector
        /// </summary>
        /// <typeparam name="TSexprRoot">Type of s-expression to produce</typeparam>
        /// <param name="reader">The current reader</param>
        /// <param name="prefix">Not used</param>
        /// <param name="c">The character triggering this hook</param>
        /// <param name="pos">Position of character that triggered this hook</param>
        /// <returns>A bit vector representing the encoded binary number</returns>
        public static TSexprRoot BitVectorLiteralHook<TSexprRoot>(SexprReader<TSexprRoot> reader, int prefix, char c, SexprPosition? pos)
        {
            // We expect a sequence of 1's and 0's (only). We read all remaining constituents and report errors if not the case
            string bitvector = reader.ConsumeConstituents();

            BitArray bv = new(bitvector.Length);
            for (int i = 0; i < bitvector.Length; ++i)
            {
                char bit = bitvector[i];
                int bvIx = bitvector.Length - i - 1; // The first character in the string is the MSb
                
                switch (bit)
                {
                    case '0':
                        bv.Set(bvIx, false);
                        break;
                    case '1':
                        bv.Set(bvIx, true);
                        break;
                    default:
                        throw new InvalidOperationException($"Expected bit vector of 1s and 0s, but got '{bit}' in: {bitvector}");
                }
            }
            return reader.SexprFactory.ConstructBitVector(bv, pos);
        }
    }
}
