using System;
using System.Collections;
using System.Collections.Generic;

namespace Semgus.Sexpr.Reader
{
    /// <summary>
    /// Factory for creating various types of S-expressions
    /// </summary>
    /// <typeparam name="TSexprRoot">Root of S-expression hierarchy</typeparam>
    public interface ISexprFactory<TSexprRoot>
    {
        /// <summary>
        /// Constructs an S-expression representing a whole number, including 0
        /// </summary>
        /// <param name="value">The value of the number</param>
        /// <param name="position">The position it was read from</param>
        /// <returns>The constructed numeral</returns>
        TSexprRoot ConstructNumeral(long value, SexprPosition? position = null);

        /// <summary>
        /// Constructs an S-expression representing a real number
        /// </summary>
        /// <param name="value">The value of the number</param>
        /// <param name="position">The position it was read from</param>
        /// <returns>The constructed decimal</returns>
        TSexprRoot ConstructDecimal(double value, SexprPosition? position = null);

        /// <summary>
        /// Constructs an S-expression representing a bit vector
        /// </summary>
        /// <param name="bv">The bit vector</param>
        /// <param name="position">The position it was read from</param>
        /// <returns>The constructed bit vector</returns>
        TSexprRoot ConstructBitVector(BitArray bv, SexprPosition? position = null);

        /// <summary>
        /// Constructs an S-expression representing a symbol
        /// </summary>
        /// <param name="name">The symbol name</param>
        /// <param name="position">The position it was read from</param>
        /// <returns>The constructed symbol</returns>
        TSexprRoot ConstructSymbol(ReadOnlySpan<char> name, SexprPosition? position = null);

        /// <summary>
        /// Constructs an S-expression representing a keyword
        /// </summary>
        /// <param name="keyword">The keyword name, not including the leading colon</param>
        /// <param name="position">The position it was read from</param>
        /// <returns>The constructed keyword</returns>
        TSexprRoot ConstructKeyword(ReadOnlySpan<char> keyword, SexprPosition? position = null);

        /// <summary>
        /// Constructs an S-expression representing a string
        /// </summary>
        /// <param name="str">The string value</param>
        /// <param name="position">The position it was read from</param>
        /// <returns>The constructed string</returns>
        TSexprRoot ConstructString(ReadOnlySpan<char> str, SexprPosition? position = null);

        /// <summary>
        /// Constructs an S-expression representing a list
        /// </summary>
        /// <param name="list">The list of S-expresions</param>
        /// <param name="position">The position it was read from</param>
        /// <returns>The constructed list</returns>
        TSexprRoot ConstructList(IList<TSexprRoot> list, SexprPosition? position = null);

        /// <summary>
        /// Constructs an S-expression representing nil, a.k.a. the empty list
        /// </summary>
        /// <param name="position">The position it was read from</param>
        /// <returns>The constructed nil</returns>
        TSexprRoot ConstructNil(SexprPosition? position = null);

        /// <summary>
        /// Constructs an S-expression representing a cons cell
        /// </summary>
        /// <param name="head">The head of the cell</param>
        /// <param name="tail">The tail of the cell</param>
        /// <param name="position">The position it was read from</param>
        /// <returns>The constructed cons cell</returns>
        TSexprRoot ConstructCons(TSexprRoot head, TSexprRoot tail, SexprPosition? position = null);

        /// <summary>
        /// Constructs an S-expression representing some arbitrary and unique value that can be used by the reader
        /// </summary>
        /// <param name="identifier">An indentifier for this sentinel</param>
        /// <returns>The constructed sentinel</returns>
        TSexprRoot ConstructSentinel(string identifier);
    }
}
