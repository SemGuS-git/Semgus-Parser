using System.Collections.Generic;

using Semgus.Parser.Forms;
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
        public Annotation Annotation { get; }
        public IReadOnlyList<Annotation> ElementAnnotations { get; }

        public SemanticRelationInstance(SemanticRelationDeclaration relation, Annotation annotation, IReadOnlyList<VariableDeclaration> elements, IReadOnlyList<Annotation> elementAnnotations) {
            Relation = relation;
            Annotation = annotation;
            Elements = elements;
            ElementAnnotations = elementAnnotations;
        }

        public void AssertCorrectness() {
            this.Assert(Elements.Count == Relation.ElementTypes.Count, $"Semantic relation {Relation.Name} must be instantiated with {Relation.ElementTypes.Count} elements");
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}