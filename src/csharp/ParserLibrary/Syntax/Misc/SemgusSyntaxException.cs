using System;

using Semgus.Parser.Reader;

namespace Semgus.Syntax {

    public class SemgusSyntaxException : Exception {
        public SemgusParserContext ParserContext { get; }
        
        public SemgusSyntaxException(ISyntaxNode node, string message) : base($"Syntax error from {node.GetType().Name} at {node.ParserContext.Start.Line}:{node.ParserContext.Start.Column}: {message}"){
            this.ParserContext = node.ParserContext;
        }
        
        public SemgusSyntaxException(SemgusParserContext parserContext, string message) : base($"Syntax error at {parserContext.Start.Line}:{parserContext.Start.Column}: {message}") {

        }
    }
}