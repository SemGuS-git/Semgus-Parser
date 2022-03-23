using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt {
    public static class SmtCommonIdentifiers {
        public static SmtIdentifier SORT_BOOL { get; } = new("Bool");
        public static SmtIdentifier SORT_INT { get; } = new("Int");
        public static SmtIdentifier SORT_STRING { get; } = new("String");
        public static SmtIdentifier SORT_REAL { get; } = new("Real");

        public static SmtIdentifier FN_AND { get; } = new("and");
        public static SmtIdentifier FN_OR { get; } = new("or");
        public static SmtIdentifier FN_EQ { get; } = new("=");
    }
}
