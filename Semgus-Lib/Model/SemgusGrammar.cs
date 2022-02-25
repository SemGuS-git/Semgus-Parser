using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt;

namespace Semgus.Model
{
    public class SemgusGrammar
    {
        public IReadOnlyCollection<NonTerminal> NonTerminals { get; }

        public IReadOnlyCollection<Production> Productions { get; }

        public SemgusGrammar(IEnumerable<NonTerminal> nonTerminals, IEnumerable<Production> productions)
        {
            NonTerminals = nonTerminals.ToList();
            Productions = productions.ToList();
        }

        public record NonTerminal(SmtIdentifier Name, SemgusTermType Sort)
        {
            public override string ToString()
            {
                return $"{Name} [{Sort.Name}]";
            }
        }

        public record Production(NonTerminal Instance, SemgusTermType.Constructor Constructor, IReadOnlyList<NonTerminal?> Occurrences)
        {
            public override string ToString()
            {
                if (Constructor.Children.Length == 0)
                {
                    return $"{Instance.Name} --> {Constructor.Operator}";
                }
                else
                {
                    return $"{Instance.Name} --> ({Constructor.Operator} {string.Join(' ', Occurrences.Select(o => o?.Name))})";
                }
            }
        }
        // N.B.: a nonterminal occurrence can be null if that occurrence isn't a term, i.e. a discovered constant
    }
}
