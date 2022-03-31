#nullable enable

namespace Semgus.Model.Smt {
    public static class SmtTheory {
        /// <summary>
        /// Everything that is defined by the user in a problem
        /// </summary>
        public static ISmtTheory UserDefined { get; } = null;
    }
}
