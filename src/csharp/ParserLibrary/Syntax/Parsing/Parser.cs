using Antlr4.Runtime;
using Semgus.Parser.Internal;
using System.IO;
using System.Text;

namespace Semgus.Syntax {
    public static class Parser {
        public static (SemgusProblem, LanguageEnvironment) ParseFileToAst(string filename) {
            SemgusLexer lexer = new SemgusLexer(new AntlrFileStream(filename));
            SemgusParser parser = new SemgusParser(new CommonTokenStream(lexer));

            var cst = parser.start();
            var normalizer = new SyntaxNormalizer();

            try { 
                return normalizer.Normalize(cst);
            } catch (SemgusSyntaxException e) {
                using (var file = new StreamReader(filename)) {
                    var exception = new FileContextSemgusSyntaxException(e.ParserContext,e.Message,GetFileContextString(e,file));
                    throw exception;
                }
            }
        }

        private static string GetFileContextString(SemgusSyntaxException e, StreamReader file) {
            var sb = new StringBuilder();
            var l0 = e.ParserContext.Start.Line;
            var l1 = e.ParserContext.Stop.Line;
            int k = 0;
            string line;
            while ((line = file.ReadLine()) != null) {
                if (k == (l0 - 1)) {
                    sb.AppendLine("--- BEGIN ERROR SECTION ---");
                } else if (k == l1) {
                    sb.AppendLine("---  END ERROR SECTION  ---");
                }
                sb.AppendLine(line);
                k++;
            }
            return sb.ToString();
        }
    }
}