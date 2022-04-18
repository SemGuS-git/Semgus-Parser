using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// A particular theory
    /// </summary>
    public interface ISmtTheory
    {
        /// <summary>
        /// The name of this theory
        /// </summary>
        SmtIdentifier Name { get; }

        [Obsolete("This member will be updated or removed to support additional theories. Use SmtContext to look up specific sorts.")]
        IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts { get; }
        IReadOnlySet<SmtIdentifier> PrimarySortSymbols { get; }
        bool TryGetSort(SmtSortIdentifier sortId, [NotNullWhen(true)] out SmtSort? resolvedSort);

        [Obsolete("This member will be updated or removed to support additional theories. Use SmtContext to look up specific functions.")]
        IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions { get; }
        IReadOnlySet<SmtIdentifier> PrimaryFunctionSymbols { get; }
        bool TryGetFunction(SmtIdentifier functionId, [NotNullWhen(true)] out SmtFunction? resolvedFunction);
    }
}
