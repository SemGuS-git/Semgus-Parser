using Antlr4.Runtime;

namespace Semgus.Syntax {
    public class FileContextSemgusSyntaxException : SemgusSyntaxException {
        public string FileContext { get; }

        public FileContextSemgusSyntaxException(ParserRuleContext parserContext, string message, string fileContext) : base(parserContext, message) {
            this.FileContext = fileContext;
        }

    }
}