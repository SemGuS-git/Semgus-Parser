using System.Collections.Generic;
using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Specifies the production rules for a single nonterminal with an associated semantic relation.
    /// This is currently also the site at which the semantic relation is defined.
    /// </summary>
    public class Production : ISyntaxNode {
        public ParserRuleContext ParserContext { get; set; }
        public Nonterminal Nonterminal { get; }
        public VariableClosure Closure { get; }
        public SemanticRelationInstance RelationInstance { get; } // CHC conclusion
        public IReadOnlyList<ProductionRule> ProductionRules { get; } // CHC premises

        public Production( Nonterminal nonterminal, VariableClosure closure, SemanticRelationInstance relationInstance, IReadOnlyList<ProductionRule> productionRules) {
            this.Nonterminal = nonterminal;
            this.Closure = closure;
            this.RelationInstance = relationInstance;
            this.ProductionRules = productionRules;
            
            this.Assert(productionRules.Count>0, "Production must have at least one rule");
        }
        
        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}