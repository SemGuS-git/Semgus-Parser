using Antlr4.Runtime;

namespace Semgus.Syntax {
    /// <summary>
    /// Unique identifier for a nonterminal
    /// </summary>
    public class Nonterminal {
        public string Name { get; }

        public Nonterminal(string name) {
            Name = name;
        }
    }
}