using Semgus.Parser.Internal;

namespace Semgus.Syntax {
    public static class VariableClosureExtensions {
        public static VariableDeclaration Resolve(this VariableClosure closure, SemgusParser.SymbolContext context) {
            var text = context.GetText();
            if(closure.TryResolve(text, out var value)) return value;
            else throw new SemgusSyntaxException(context,$"Unable to resolve variable \"{text}\"");
        }
    }
}