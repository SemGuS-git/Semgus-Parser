namespace Semgus.Syntax {
    public static class SyntaxNodeExtensions{
        public static void Assert(this ISyntaxNode node, bool condition, string message){
            if(!condition) throw new SemgusSyntaxException(node,message);
        }
    }
}