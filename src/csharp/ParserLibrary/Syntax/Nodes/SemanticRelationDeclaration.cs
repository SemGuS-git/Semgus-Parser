using System.Collections.Generic;

using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    // Defines a semantic relation
    // e.g., Start.Sem(Term Int Int Int)
    public class SemanticRelationDeclaration : ISyntaxNode {
        public SemgusParserContext ParserContext { get; set; }
        public string Name { get; }
        public IReadOnlyList<SemgusType> ElementTypes { get; }

        public SemanticRelationDeclaration(string name, IReadOnlyList<SemgusType> elementTypes) {
            Name = name;
            ElementTypes = elementTypes;
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}