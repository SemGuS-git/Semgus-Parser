using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Semgus.Parser
{
    /// <summary>
    /// Utilities for generating symbols
    /// </summary>
    internal class GensymUtils
    {
        /// <summary>
        /// Atomically increasing counter for symbols
        /// </summary>
        private static long _gensym_counter = 0;

        /// <summary>
        /// Creates a fresh symbol with the given prefix
        /// </summary>
        /// <param name="prefix">Symbol prefix. Defaults to _G</param>
        /// <returns>A fresh identifier</returns>
        public static SmtIdentifier Gensym(string prefix = "_G")
        {
            long ix = Interlocked.Increment(ref _gensym_counter);
            return new SmtIdentifier(prefix, new SmtIdentifier.Index(ix));
        }
    }
}
