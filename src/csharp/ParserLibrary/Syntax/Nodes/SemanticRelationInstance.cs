using System.Collections.Generic;

using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    /// <summary>
    /// Describes the inclusion of a particular tuple of variables in a semantic relation.
    /// This appears as the conclusion of a production CHC.
    /// </summary>
    public class SemanticRelationInstance : ISyntaxNode {
        public SemgusParserContext ParserContext { get; set; }
        public SemanticRelationDeclaration Relation { get; }
        public IReadOnlyList<VariableDeclaration> Elements { get; }

        public SemanticRelationInstance(SemanticRelationDeclaration relation, IReadOnlyList<VariableDeclaration> elements) {
            Relation = relation;
            Elements = elements;
        }

        public void AssertCorrectness() {
            this.Assert(Elements.Count == Relation.ElementTypes.Count, $"Semantic relation {Relation.Name} must be instantiated with {Relation.ElementTypes.Count} elements");
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}