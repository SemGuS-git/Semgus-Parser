using System.Collections.Generic;
using Semgus.Parser.Internal;

namespace Semgus.Syntax {
    public static class LanguageEnvironmentExtensions {
        public static SemgusType IncludeType(this LanguageEnvironment env, SemgusParser.TypeContext context) => env.IncludeType(context.GetText());
        public static SemgusType IncludeType(this LanguageEnvironment env, SemgusParser.SymbolContext context) => env.IncludeType(context.GetText());
        
        public static SemanticRelationDeclaration ResolveRelation(this LanguageEnvironment env, SemgusParser.SymbolContext context) {
            var txt = context.GetText();
            try {
                return env.ResolveRelation(txt);
            } catch(KeyNotFoundException) {
                throw new SemgusSyntaxException(context,$"Unable to resolve relation \"{txt}\"");
            }
        }
        
        public static Nonterminal ResolveNonterminal(this LanguageEnvironment env, SemgusParser.SymbolContext context) {
            var txt = context.GetText();
            try {
                return env.ResolveNonterminal(txt);
            } catch(KeyNotFoundException) {
                throw new SemgusSyntaxException(context,$"Unable to resolve nonterminal \"{txt}\"");
            }
        }
        
        public static SemgusType ResolveType(this LanguageEnvironment env, SemgusParser.TypeContext context) {
            var txt = context.GetText();
            try {
                return env.ResolveType(txt);
            } catch(KeyNotFoundException) {
                throw new SemgusSyntaxException(context,$"Unable to resolve type \"{txt}\"");
            }
        }
    }
}