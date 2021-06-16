using System.Collections.Generic;
using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Specifies the production rules for a single nonterminal with an associated semantic relation.
    /// This is currently also the site at which the semantic relation is defined.
    /// </summary>
    public class ProductionGroup : ISyntaxNode {
        public ParserRuleContext ParserContext { get; set; }
        public Nonterminal Nonterminal { get; }
        public VariableClosure Closure { get; }
        public SemanticRelationInstance RelationInstance { get; } // CHC conclusion
        public IReadOnlyList<SemanticRule> SemanticRules { get; } // CHC premises

        public ProductionGroup(Nonterminal nonterminal, VariableClosure closure, SemanticRelationInstance relationInstance, IReadOnlyList<SemanticRule> semanticRules) {
            this.Nonterminal = nonterminal;
            this.Closure = closure;
            this.RelationInstance = relationInstance;
            this.SemanticRules = semanticRules;
        }

        public void AssertCorrectness() {
            this.Assert(SemanticRules.Count > 0, "Production group must contain at least one rule");
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}