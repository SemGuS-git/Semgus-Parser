using System;
using System.Collections.Generic;
using System.Linq;

namespace Semgus.Sexpr.Reader
{
    /// <summary>
    /// Holds information around how to interpret S-expressions as text
    /// </summary>
    /// <typeparam name="TSexprRoot">Root type of S-expressions</typeparam>
    public class Readtable<TSexprRoot>
    {
        /// <summary>
        /// The casing mode to use
        /// </summary>
        public ReadtableCase Case { get; set; }

        /// <summary>
        /// Creates a new and empty readtable
        /// </summary>
        private Readtable()
        {
            _dispatchTable = new Dictionary<char, MacroHook<TSexprRoot>>();
            _dispatchMacroTable = new Dictionary<char, IDictionary<char, DispatchMacroHook<TSexprRoot>>>();
        }

        /// <summary>
        /// Creates a readtable with a copy of another's dispatch table
        /// </summary>
        /// <param name="from">Table to create from</param>
        private Readtable(Readtable<TSexprRoot> from)
        {
            _dispatchTable = new Dictionary<char, MacroHook<TSexprRoot>>(from._dispatchTable);
            _dispatchMacroTable = new Dictionary<char, IDictionary<char, DispatchMacroHook<TSexprRoot>>>(from._dispatchMacroTable
                .ToDictionary(kvp => kvp.Key, kvp => new Dictionary<char, DispatchMacroHook<TSexprRoot>>(kvp.Value) as IDictionary<char, DispatchMacroHook<TSexprRoot>>));
        }

        /// <summary>
        /// The macro dispatch table. Maps characters to macro hooks
        /// </summary>
        private readonly IDictionary<char, MacroHook<TSexprRoot>> _dispatchTable;

        /// <summary>
        /// The dispatch macro table. Maps characters to dispatch tables
        /// </summary>
        private readonly IDictionary<char, IDictionary<char, DispatchMacroHook<TSexprRoot>>> _dispatchMacroTable;

        /// <summary>
        /// Sets a macro hook for a given character. This also changes the syntax type of said character.
        /// </summary>
        /// <param name="c">The character to set macro hook for</param>
        /// <param name="hook">The macro hook</param>
        public void SetMacroCharacter(char c, MacroHook<TSexprRoot> hook)
        {
            _dispatchTable.Add(c, hook);
        }

        /// <summary>
        /// Returns the macro hook associated with the given character
        /// </summary>
        /// <param name="c">Character to get hook for</param>
        /// <returns>The macro hook</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if not a macro character</exception>
        public MacroHook<TSexprRoot> GetMacroCharacter(char c)
        {
            if (_dispatchTable.TryGetValue(c, out var hook))
            {
                return hook;
            }
            else
            {
                throw new InvalidOperationException("Attempt to get macro hook for non-macro character");
            }
        }

        /// <summary>
        /// Function used for dispatching macro hooks
        /// </summary>
        /// <param name="reader">The current reader</param>
        /// <param name="c">The dispatching macro character</param>
        /// <param name="pos">The position of the dispatching character</param>
        /// <returns>The read s-expression from the associated dispatch hook</returns>
        private TSexprRoot DispatchFunction(SexprReader<TSexprRoot> reader, char c, SexprPosition? pos)
        {
            if (_dispatchMacroTable.TryGetValue(c, out var table))
            {
                if (!reader.TryReadCharacter(out char subChar, out var st))
                {
                    throw new Exception("EndofStream when dispatching");
                }
                if (table.TryGetValue(subChar, out var hook))
                {
                    return hook(reader, default /* TODO, but YAGNI */, subChar, pos);
                }
                else
                {
                    throw new InvalidOperationException("Not a subcharacter of '" + c + "': '" + subChar + "'");
                }
            }
            else
            {
                throw new InvalidOperationException("Not a dispatching character: " + c);
            }
        }

        /// <summary>
        /// Makes the given character a dispatching macro character
        /// </summary>
        /// <param name="dispatchChar">Character to turn into a dispatching macro character</param>
        public void MakeDispatchMacroCharacter(char dispatchChar)
        {
            _dispatchMacroTable.Add(dispatchChar, new Dictionary<char, DispatchMacroHook<TSexprRoot>>());
            SetMacroCharacter(dispatchChar, DispatchFunction);
        }

        /// <summary>
        /// Sets the dispatch hook associated with a given dispatching macro character and sub character.
        /// E.g., for #xFFFF, sharpsign is the dispatching character and x is the sub character
        /// </summary>
        /// <param name="dispatchChar">The dispatching macro character to register a hook for</param>
        /// <param name="subChar">The sub character to register a hook for</param>
        /// <param name="hook">The dispatch hook to register</param>
        public void SetDispatchMacroCharacter(char dispatchChar, char subChar, DispatchMacroHook<TSexprRoot> hook)
        {
            if (GetSyntaxFromChar(dispatchChar) != SyntaxType.Macro || !_dispatchMacroTable.ContainsKey(dispatchChar))
            {
                throw new InvalidOperationException("Not a macro character: '" + dispatchChar + "'");
            }

            _dispatchMacroTable[dispatchChar].Add(subChar, hook);
        }

        /// <summary>
        /// Gets the syntax type of the character. That is, what role does the character play in the syntax?
        /// </summary>
        /// <param name="c">Character to get type of</param>
        /// <returns>Syntax type of character</returns>
        public SyntaxType GetSyntaxFromChar(char c)
        {
            // Whitespace - tab, lf, cr, or space. This is stricter than C#'s whitespace definition
            if ('\t' == c || '\r' == c || '\n' == c || ' ' == c)
            {
                return SyntaxType.Whitespace;
            }

            // Macro characters - do we have an entry in the hook table?
            if (_dispatchTable.ContainsKey(c))
            {
                return SyntaxType.Macro;
            }

            // Constituents, a.k.a. printable characters
            if ((32 <= c && c <= 126) || 128 <= c)
            {
                return SyntaxType.Constituent;
            }

            // Invalid - everything else
            return SyntaxType.Invalid;
        }

        /// <summary>
        /// Array of symbol characters that are "alphabetic" constituents
        /// </summary>
        private static readonly char[] _alphabeticSymbols = { '~', '!', '@', '$', '%', '^', '&', '*', '-', '_', '+', '=', '<', '>', '.', '?', '/' };

        /// <summary>
        /// Returns the constituent traits of the given character. That is, what role the character plays in symbols
        /// </summary>
        /// <param name="c">The character to check</param>
        /// <returns>The constituent trait (or traits)</returns>
        public ConstituentTrait GetConstituentTrait(char c)
        {
            ConstituentTrait type = ConstituentTrait.Invalid;
            if ('0' == c)
            {
                type |= ConstituentTrait.Zero;
            }

            if ('.' == c)
            {
                type |= ConstituentTrait.DecimalPoint;
            }

            if (':' == c)
            {
                type |= ConstituentTrait.PackageMarker;
            }

            if ('0' <= c && c <= '9')
            {
                type |= ConstituentTrait.Digit;
                type |= ConstituentTrait.Alphabetic; // as digits are valid in symbols, too
            }
            else if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z'))
            {
                type |= ConstituentTrait.Alphabetic;
            }
            else if (_alphabeticSymbols.Contains(c))
            {
                type |= ConstituentTrait.Alphabetic;
            }

            return type;
        }

        /// <summary>
        /// Creates a copy of this readtable
        /// </summary>
        /// <returns>A copy of this readtable</returns>
        public Readtable<TSexprRoot> Copy() => new(this);

        /// <summary>
        /// Gets a readtable with sensible defaults. No dispatching macros are configured;
        /// however, sharpsign is marked as a dispatcher and ready to go.
        /// </summary>
        /// <returns></returns>
        public static Readtable<TSexprRoot> GetDefaultReadtable()
        {
            Readtable<TSexprRoot> table = new();

            table.SetMacroCharacter('(', ReaderHooks.LParenHook);
            table.SetMacroCharacter(')', ReaderHooks.RParenHook);
            table.SetMacroCharacter(';', ReaderHooks.CommentHook);
            table.SetMacroCharacter('|', ReaderHooks.EscapeHook);
            table.SetMacroCharacter('"', ReaderHooks.StringHook);

            table.MakeDispatchMacroCharacter('#');
            return table;
        }
    }
}
