using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Describes a boolean function that evaluates whether a tuple of formula terms is included in a relation.
    /// This may appear inside of production rule predicates or constraints.
    /// </summary>
    public class SemanticRelationQuery : IFormula {
        public ParserRuleContext ParserContext { get; set; }
        public SemanticRelationDeclaration Relation { get; }
        public IReadOnlyList<IFormula> Terms { get; }

        public SemanticRelationQuery(SemanticRelationDeclaration relation, IReadOnlyList<IFormula> terms) {
            Relation = relation;
            Terms = terms;
        }

        public void AssertCorrectness() {
            this.Assert(Terms.Count == Relation.ElementTypes.Count, $"Semantic relation {Relation.Name} must be queried with {Relation.ElementTypes.Count} elements (found {Terms.Count})");
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);

        public string PrintFormula() => $"({Relation.Name} {string.Join(" ", Terms.Select(t => t.PrintFormula()))})";
    }
}