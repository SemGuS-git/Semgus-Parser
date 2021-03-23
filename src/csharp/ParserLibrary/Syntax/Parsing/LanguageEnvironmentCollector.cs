using System.Collections.Specialized;
using System.Linq;
using Antlr4.Runtime.Misc;
using Semgus.Parser.Internal;

namespace Semgus.Syntax {
    /// <summary>
    /// Visitor that collects user-defined identifiers as part of the name analysis process.
    /// </summary>
    public class LanguageEnvironmentCollector : SemgusBaseVisitor<LanguageEnvironment> {
        private readonly LanguageEnvironment _env = new LanguageEnvironment();

        protected override LanguageEnvironment DefaultResult => _env;

        public override LanguageEnvironment VisitVar_decl([NotNull] SemgusParser.Var_declContext context) {
            _env.IncludeType(context.type());
            return _env;
        }

        public override LanguageEnvironment VisitNt_name([NotNull] SemgusParser.Nt_nameContext context) {
            _env.IncludeNonterminal(context.symbol());
            return _env;
        }

        public override LanguageEnvironment VisitNt_relation_def([NotNull] SemgusParser.Nt_relation_defContext context) {
            var symbols = context.symbol();
            _env.AddNewSemanticRelation(symbols[0], symbols.Skip(1).Select(_env.IncludeType).ToList());
            return _env;
        }
    }
}