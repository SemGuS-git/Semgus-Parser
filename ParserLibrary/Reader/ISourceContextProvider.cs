using Semgus.Sexpr.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// Provides context information (i.e., text) for a given error position
    /// </summary>
    internal interface ISourceContextProvider
    {
        /// <summary>
        /// Tries to get the full line of where an error occurred
        /// </summary>
        /// <param name="position">Position to look up</param>
        /// <returns>Source line</returns>
        bool TryGetSourceLine(SexprPosition? position, out string? line);
    }
}
