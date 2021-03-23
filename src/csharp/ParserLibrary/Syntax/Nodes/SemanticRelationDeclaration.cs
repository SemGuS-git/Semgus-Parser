using System.Collections.Generic;
using Antlr4.Runtime;

namespace Semgus.Syntax {
    // Defines a semantic relation
    // e.g., Start.Sem(Term Int Int Int)
    public class SemanticRelationDeclaration : ISyntaxNode {
        public ParserRuleContext ParserContext { get; }
        public string Name { get; }
        public IReadOnlyList<SemgusType> ElementTypes { get; }

        public SemanticRelationDeclaration(ParserRuleContext parserContext, string name, IReadOnlyList<SemgusType> elementTypes) {
            ParserContext = parserContext;
            Name = name;
            ElementTypes = elementTypes;
        }
        
        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}