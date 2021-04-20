using System;
using Antlr4.Runtime;

namespace Semgus.Syntax {

    public class SemgusSyntaxException : Exception {
        public ParserRuleContext ParserContext { get; }
        
        public SemgusSyntaxException(ISyntaxNode node, string message) : base($"Syntax error from {node.GetType().Name} at {node.ParserContext.Start.Line}:{node.ParserContext.Start.Column}: {message}"){
            this.ParserContext = node.ParserContext;
        }
        
        public SemgusSyntaxException(ParserRuleContext parserContext, string message) : base($"Syntax error at {parserContext.Start.Line}:{parserContext.Start.Column}: {message}") {

        }
    }
}