using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// Interface for converting between S-expression tokens and SMT objects
    /// </summary>
    public interface ISmtConverter
    {
        /// <summary>
        /// Converts an object of type tFrom to an object of type tTo. Usually, from is an 
        /// S-expression token and to is an SMT object
        /// </summary>
        /// <param name="tFrom">Type to convert from</param>
        /// <param name="tTo">Type to convert to</param>
        /// <param name="from">Object to convert from</param>
        /// <param name="to">Conversion result</param>
        /// <returns>True if successfully converted</returns>
        public bool TryConvert(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to);


        /// <summary>
        /// Converts an object from to an object to. Usually, from is an 
        /// S-expression token and to is an SMT object. The to and from types are inferred
        /// from the generic parameters.
        /// </summary>
        /// <param name="from">Object to convert from</param>
        /// <param name="to">Conversion result</param>
        /// <returns>True if successfully converted</returns>
        public bool TryConvert<TTarget>(object from, [NotNullWhen(true)] out TTarget? to);
    }
}
