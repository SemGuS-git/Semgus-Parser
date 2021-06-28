using System.IO;
using System.Text;

namespace Semgus.Syntax {
    public static class Parser {
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