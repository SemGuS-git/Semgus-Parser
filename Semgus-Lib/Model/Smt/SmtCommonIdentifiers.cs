using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt {
    public static class SmtCommonIdentifiers {
        public static SmtIdentifier CoreTheoryId { get; } = new("CoreTheory");
        public static SmtIdentifier IntsTheoryId { get; } = new("IntsTheory");
        public static SmtIdentifier StringsTheoryId { get; } = new("StringsTheory");

        public static SmtSortIdentifier BoolSortId { get; } = new("Bool");
        public static SmtSortIdentifier IntSortId { get; } = new("Int");
        public static SmtSortIdentifier StringSortId { get; } = new("String");
        public static SmtSortIdentifier RealSortId { get; } = new("Real");

        public static SmtIdentifier AndFunctionId { get; } = new("and");
        public static SmtIdentifier OrFunctionId { get; } = new("or");
        public static SmtIdentifier EqFunctionId { get; } = new("=");
    }
}
