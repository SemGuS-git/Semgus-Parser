using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    /// <summary>
    /// An SMT object that can be applied in a function call
    /// </summary>
    public interface IApplicable
    {
        public SmtIdentifier Name { get; }
        public ISmtTheory Theory { get; }
        public bool TryResolveRank([NotNullWhen(true)] out SmtFunctionRank? rank, SmtSort? returnSort, params SmtSort[] argumentSorts);

        /// <summary>
        /// Gets an informative string about available ranks
        /// </summary>
        /// <returns>Rank information string</returns>
        public string GetRankHelp();

        /// <summary>
        /// Checks if there is a possible resolution for the given arity
        /// </summary>
        /// <param name="arity">Arity to check</param>
        /// <returns>True if there is a valid rank with the given arity</returns>
        public bool IsArityPossible(int arity);
    }
}
