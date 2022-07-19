
using System.Diagnostics.CodeAnalysis;

namespace Semgus.Model.Smt {
    public static class SmtTheory {
        /// <summary>
        /// Everything that is defined by the user in a problem
        /// </summary>
        public static ISmtTheory UserDefined { get; } = new UserDefinedTheory();

        /// <summary>
        /// Holder class representing user-defined objects
        /// </summary>
        private class UserDefinedTheory : ISmtTheory
        {
            public SmtIdentifier Name => new("UserDefined");

            public IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts => throw new NotImplementedException();

            public IReadOnlyDictionary<SmtIdentifier, IApplicable> Functions => throw new NotImplementedException();

            public IReadOnlySet<SmtIdentifier> PrimarySortSymbols => throw new NotImplementedException();

            public IReadOnlySet<SmtIdentifier> PrimaryFunctionSymbols => throw new NotImplementedException();

            public bool TryGetFunction(SmtIdentifier functionId, [NotNullWhen(true)] out IApplicable? resolvedFunction)
            {
                throw new NotImplementedException();
            }

            public bool TryGetSort(SmtSortIdentifier sortId, [NotNullWhen(true)] out SmtSort? resolvedSort)
            {
                throw new NotImplementedException();
            }
        }
    }
}
