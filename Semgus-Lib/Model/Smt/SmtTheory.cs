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
        public static SmtTheory Core;

        public static SmtTheory Ints;

        public static SmtTheory Reals;

        public static SmtTheory Reals_Ints;

        public static SmtTheory FloatingPoint;

        public static SmtTheory Strings;

        public static SmtTheory FixedSizeBitVectors;

        public static SmtTheory ArraysEx;

        /// <summary>
        /// Everything that is defined by the user in a problem
        /// </summary>
        public static SmtTheory UserDefined;

        public abstract IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts { get; }
        public abstract IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions { get; }
    }
}
