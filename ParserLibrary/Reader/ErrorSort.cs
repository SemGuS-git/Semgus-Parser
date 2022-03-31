using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt;

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// Sort representing a not-well-sorted term
    /// </summary>
    internal class ErrorSort : SmtSort
    {
        /// <summary>
        /// Singleton instance of the error sort
        /// </summary>
        public static ErrorSort Instance { get; } = new ErrorSort();

        /// <summary>
        /// Constructs an error sort
        /// </summary>
        private ErrorSort() : base(new SmtSortIdentifier("@error")) { }
    }
}
