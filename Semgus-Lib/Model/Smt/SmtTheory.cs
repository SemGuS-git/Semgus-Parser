using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Model.Smt
{
    public abstract class SmtTheory
    {
        public static SmtTheory Core = null!;

        public static SmtTheory Ints = null!;

        public static SmtTheory Reals = null!;

        public static SmtTheory Reals_Ints = null!;

        public static SmtTheory FloatingPoint = null!;

        public static SmtTheory Strings = null!;

        public static SmtTheory FixedSizeBitVectors = null!;

        public static SmtTheory ArraysEx = null!;

        /// <summary>
        /// Everything that is defined by the user in a problem
        /// </summary>
        public static SmtTheory UserDefined = null!;

        public abstract IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts { get; }
        public abstract IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions { get; }
    }
}
