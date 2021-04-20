using System.Collections.Generic;
using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Describes the inclusion of a particular tuple of variables in a semantic relation.
    /// This appears as the conclusion of a production CHC.
    /// </summary>
    public class SemanticRelationInstance : ISyntaxNode {
        public ParserRuleContext ParserContext { get; set; }
        public SemanticRelationDeclaration Relation { get; }
        public IReadOnlyList<VariableDeclaration> Elements { get; }

        public SemanticRelationInstance(SemanticRelationDeclaration relation, IReadOnlyList<VariableDeclaration> elements) {
            Relation = relation;
            Elements = elements;

            this.Assert(elements.Count == relation.ElementTypes.Count, $"Semantic relation {relation.Name} must be instantiated with {relation.ElementTypes.Count} elements");
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}