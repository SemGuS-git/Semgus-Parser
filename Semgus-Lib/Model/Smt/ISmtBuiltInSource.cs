using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// An SMT object source for objects defined by the SMT standard
    /// </summary>
    public interface ISmtBuiltInSource : ISmtSource
    {

        /// <summary>
        /// Set of primary sort symbols, i.e., simple symbols and first symbol of indexed identifiers
        /// </summary>
        IReadOnlySet<SmtIdentifier> PrimarySortSymbols { get; }

        /// <summary>
        /// Tries to get a function by name from this theory
        /// </summary>
        /// <param name="functionId">Function name to get</param>
        /// <param name="resolvedFunction">The function object</param>
        /// <returns>True if successfully gotten</returns>
        bool TryGetSort(SmtSortIdentifier sortId, [NotNullWhen(true)] out SmtSort? resolvedSort);

        /// <summary>
        /// Set of primary function symbols, i.e., simple symbols and first symbol of indexed identifiers
        /// </summary>
        IReadOnlySet<SmtIdentifier> PrimaryFunctionSymbols { get; }

        /// <summary>
        /// Tries to get a function by name from this theory
        /// </summary>
        /// <param name="functionId">Function name to get</param>
        /// <param name="resolvedFunction">The function object</param>
        /// <returns>True if successfully gotten</returns>
        bool TryGetFunction(SmtIdentifier functionId, [NotNullWhen(true)] out IApplicable? resolvedFunction);
    }
}
