using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt {
    public static class SmtCommonIdentifiers {
        public static SmtIdentifier BoolSortId { get; } = new("Bool");
        public static SmtIdentifier IntSortId { get; } = new("Int");
        public static SmtIdentifier StringSortId { get; } = new("String");
        public static SmtIdentifier RealSortId { get; } = new("Real");

        public static SmtIdentifier AndFunctionId { get; } = new("and");
        public static SmtIdentifier OrFunctionId { get; } = new("or");
        public static SmtIdentifier EqFunctionId { get; } = new("=");
    }
}
