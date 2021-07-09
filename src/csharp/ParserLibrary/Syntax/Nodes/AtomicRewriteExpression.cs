using System.Collections.Generic;

using Semgus.Parser.Reader;

namespace Semgus.Syntax {
    /// <summary>
    /// Production rewrite expression consisting of a single atomic element.
    /// </summary>
    public class AtomicRewriteExpression : IProductionRewriteExpression {
        public SemgusParserContext ParserContext { get; set; }
        public IProductionRewriteAtom Atom { get; }

        public AtomicRewriteExpression(IProductionRewriteAtom atom) {
            Atom = atom;
        }

        public bool IsLeaf() => !(Atom is NonterminalTermDeclaration);

        public IEnumerable<NonterminalTermDeclaration> GetChildTerms() {
            if (Atom is NonterminalTermDeclaration dec) yield return dec;
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);

        public string GetNamingSymbol() {
            switch (Atom) {
                case LiteralBase lit: return lit.BoxedValue.ToString();
                case LeafTerm leaf: return leaf.Text;
                default: return "";
            }
        }
    }
}