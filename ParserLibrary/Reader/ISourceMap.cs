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
