using System.Collections.Generic;
using System.Linq;

namespace Semgus.Parser {
    public class PrintAstVisitor : IAstVisitor<string> {
        public HashSet<string> symbols = new HashSet<string>();
        public HashSet<string> types = new HashSet<string>();

        private string Of(NodeBase node) => node.Accept(this);
        private string Of(string sep, NodeBase[] nodes) => string.Join(sep, nodes.Select(Of));

        public string Visit(EmptyFormulaNode node) => "()";

        public string Visit(FunctionApplicationNode node) => $"({Of(node.symbol)} {string.Join(" ", node.args.Select(x => x.Accept(this)))})";

        public string Visit(LiteralNodeBase node) => "\\" + node.ValueToString();

        public string Visit(SymbolNode node) {
            symbols.Add(node.value);
            return $"{node.value}";
        }

        public string Visit(VarDeclNode node) => $"{Of(node.symbol)}:{node.type.Accept(this)}";

        public string Visit(SynthFunNode node) => $"({Of(node.symbol)} ({Of(" ", node.inputArgs)}) ({Of(" ", node.outputArgs)})\n({Of("\n", node.productions)}\n)";

        public string Visit(StartNode node) => $"{node.synthFun.Accept(this)}\n{string.Join("\n", node.constraints.Select(x => x.Accept(this)))}";

        public string Visit(RhsNtAtomNode node) => $"{Of(node.ntName)}:{Of(node.ntTerm)}";

        public string Visit(RhsOpExpressionNode node) => $"({Of(node.op)} {Of(" ", node.terms)})";

        public string Visit(PredicateNode node) => $"[({Of(" ", node.auxVariableDefinitions)}) {Of(node.formula)}]";

        public string Visit(ProductionNode node) => $"  {Of(node.lhs)}  (\n{Of("\n\n", node.rhs)}\n  )";

        public string Visit(ProductionRhsNode node) => $"    {Of(node.expression)} {Of(node.predicate)}";

        public string Visit(TypeNode node) {
            types.Add(node.name);
            return $"`{node.name}`";
        }

        public string Visit(NtRelationNode node) => $"({Of(" ", node.auxVariableDefinitions)}) ({Of(node.semanticRelationName)} {Of(" ", node.varNames)})";

        public string Visit(NtRelationDefNode node) => $"{Of(node.semanticRelationName)} ({Of(" ", node.varTypes)})";

        public string Visit(ProductionLhsNode node) => $"  {Of(node.ntName)} ({Of(node.ntRelationDef)}) : {Of(node.ntTerm)}\n  [{Of(node.ntRelation)}]";
    }
}