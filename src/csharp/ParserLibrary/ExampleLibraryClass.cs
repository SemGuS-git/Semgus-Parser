using Antlr4.Runtime;
using Semgus.Parser.Internal;
using System;

namespace Semgus.Parser
{
    public class ExampleLibraryClass
    {
        public static int TryParseGrammar(string filename)
        {
            SemgusLexer lexer = new SemgusLexer(new AntlrFileStream(filename));
            SemgusParser parser = new SemgusParser(new CommonTokenStream(lexer));
            parser.start();
            return parser.NumberOfSyntaxErrors;
        }
    }
}
