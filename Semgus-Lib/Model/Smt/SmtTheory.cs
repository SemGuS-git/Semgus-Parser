
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

            public IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions => throw new NotImplementedException();
        }
    }
}
