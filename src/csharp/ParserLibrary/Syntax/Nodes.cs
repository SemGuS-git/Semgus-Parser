namespace Semgus.Parser {
    public abstract class NodeBase {
        public abstract T Accept<T>(IAstVisitor<T> visitor);
    }

    public class EmptyFormulaNode : NodeBase {
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class ProductionNode : NodeBase {
        public readonly NodeBase lhs;
        public readonly NodeBase[] rhs;

        public ProductionNode(NodeBase lhs, NodeBase[] rhs) {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    public class FunctionApplicationNode : NodeBase {
        public readonly NodeBase symbol;
        public readonly NodeBase[] args;

        public FunctionApplicationNode(NodeBase symbol, NodeBase[] args) {
            this.symbol = symbol;
            this.args = args;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public abstract class LiteralNodeBase : NodeBase {
        public abstract string ValueToString();
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class LiteralNode<T> : LiteralNodeBase {
        public readonly T value;

        public LiteralNode(T value) {
            this.value = value;
        }

        public override string ValueToString() => value.ToString();
    }

    public class SymbolNode : NodeBase {
        public readonly string value;

        public SymbolNode(string value) {
            this.value = value;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    public class SynthFunNode : NodeBase {
        public readonly NodeBase symbol;
        public readonly NodeBase[] inputArgs;
        public readonly NodeBase[] outputArgs;
        public readonly NodeBase[] productions;

        public SynthFunNode(NodeBase symbol, NodeBase[] inputArgs, NodeBase[] outputArgs, NodeBase[] productions) {
            this.symbol = symbol;
            this.inputArgs = inputArgs;
            this.outputArgs = outputArgs;
            this.productions = productions;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    public class VarDeclNode : NodeBase {
        public readonly NodeBase symbol;
        public readonly NodeBase type;

        public VarDeclNode(NodeBase symbol, NodeBase type) {
            this.symbol = symbol;
            this.type = type;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    public class StartNode : NodeBase {
        public readonly NodeBase synthFun;
        public readonly NodeBase[] constraints;

        public StartNode(NodeBase synthFun, NodeBase[] constraints) {
            this.synthFun = synthFun;
            this.constraints = constraints;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class RhsNtAtomNode : NodeBase {
        public readonly NodeBase ntName;
        public readonly NodeBase ntTerm;

        public RhsNtAtomNode(NodeBase ntName, NodeBase ntTerm) {
            this.ntName = ntName;
            this.ntTerm = ntTerm;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class RhsOpExpressionNode : NodeBase {
        public readonly NodeBase op;
        public readonly NodeBase[] terms;

        public RhsOpExpressionNode(NodeBase op, NodeBase[] terms) {
            this.op = op;
            this.terms = terms;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class PredicateNode : NodeBase {
        public readonly NodeBase[] auxVariableDefinitions;
        public readonly NodeBase formula;

        public PredicateNode(NodeBase[] auxVariableDefinitions, NodeBase formula) {
            this.auxVariableDefinitions = auxVariableDefinitions;
            this.formula = formula;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class ProductionRhsNode : NodeBase {
        public readonly NodeBase expression;
        public readonly NodeBase predicate;

        public ProductionRhsNode(NodeBase expression, NodeBase predicates) {
            this.expression = expression;
            this.predicate = predicates;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class TypeNode : NodeBase {
        public readonly string name;

        public TypeNode(string name) {
            this.name = name;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class NtRelationNode : NodeBase {
        public readonly NodeBase[] auxVariableDefinitions;
        public readonly NodeBase semanticRelationName;
        public readonly NodeBase[] varNames;

        public NtRelationNode(NodeBase[] auxVariableDefinitions, NodeBase semanticRelationName, NodeBase[] varNames) {
            this.auxVariableDefinitions = auxVariableDefinitions;
            this.semanticRelationName = semanticRelationName;
            this.varNames = varNames;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class NtRelationDefNode : NodeBase {
        public readonly NodeBase semanticRelationName;
        public readonly NodeBase[] varTypes;

        public NtRelationDefNode(NodeBase semanticRelationName, NodeBase[] varTypes) {
            this.semanticRelationName = semanticRelationName;
            this.varTypes = varTypes;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class ProductionLhsNode : NodeBase {
        public readonly NodeBase ntName;
        public readonly NodeBase ntRelationDef;
        public readonly NodeBase ntTerm;
        public readonly NodeBase ntRelation;

        public ProductionLhsNode(NodeBase ntName, NodeBase ntRelationDef, NodeBase ntTerm, NodeBase ntRelation) {
            this.ntName = ntName;
            this.ntRelationDef = ntRelationDef;
            this.ntTerm = ntTerm;
            this.ntRelation = ntRelation;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}