using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt;

namespace Semgus.Model
{
    /// <summary>
    /// A grammar specification for a SemGuS problem
    /// </summary>
    public class SemgusGrammar
    {
        /// <summary>
        /// All non-terminals in this grammar. The first non-terminal is the starting symbol
        /// </summary>
        public IReadOnlyCollection<NonTerminal> NonTerminals { get; }

        /// <summary>
        /// All productions in this grammar
        /// </summary>
        public IReadOnlyCollection<Production> Productions { get; }

        /// <summary>
        /// Creates a new grammar with the given non-terminals and productions.
        /// All non-terminals referenced in the productions must be present in the list of non-terminals
        /// </summary>
        /// <param name="nonTerminals">List of non-terminals</param>
        /// <param name="productions">List of productions</param>
        public SemgusGrammar(IEnumerable<NonTerminal> nonTerminals, IEnumerable<Production> productions)
        {
            NonTerminals = nonTerminals.ToList();
            Productions = productions.ToList();
        }

        /// <summary>
        /// A non-terminal
        /// </summary>
        /// <param name="Name">The non-terminal's name</param>
        /// <param name="Sort">The sort of terms produced by the non-terminal</param>
        public record NonTerminal(SmtIdentifier Name, SemgusTermType Sort)
        {
            public override string ToString()
            {
                return $"{Name} [{Sort.Name}]";
            }
        }

        /// <summary>
        /// A production
        /// </summary>
        /// <param name="Instance">The left-hand-side of the production</param>
        /// <param name="Constructor">The constructor (a.k.a. operator) for the production</param>
        /// <param name="Occurrences">The children of the operator, a.k.a. the right-hand-side non-terminals</param>
        /// <remarks>
        /// Both the constructor and the occurrences are nullable, for the following situations.
        /// 
        /// The constructor will be null when this production is a "no-op" non-terminal to non-terminal 
        /// production that maps two non-terminals withthe same term type, e.g.:
        /// ((Start E) (A E)) ((Start E (A ...)) (A E (...)))
        /// In this case, the occurrences list will have only a single element, the RHS non-terminal.
        /// 
        /// Occurrences will be null when they correspond to a constant, non-term-type value, making the
        /// production represent a family of productions, one for each constant value. For example, a term
        /// that can produce any integer value would have a constructor taking an int, and a single null
        /// value in the occurrences list representing this. Here, the production would represent an infinite
        /// family of productions, one for every possible integer value.
        /// 
        /// Note that both these situations will not occur at the same time; since our non-terminals must be
        /// of sort 'TermType', they cannot produce a constant value. Though this does need to be revisited
        /// at some point so that users can specify constants in the grammar directly.
        /// </remarks>
        public record Production(NonTerminal Instance, SemgusTermType.Constructor? Constructor, IReadOnlyList<NonTerminal?> Occurrences)
        {
            /// <summary>
            /// Constructs a non-terminal to non-terminal production
            /// </summary>
            /// <param name="instance">LHS non-terminal</param>
            /// <param name="occurrence">RHS non-terminal</param>
            public Production(NonTerminal instance, NonTerminal occurrence)
                : this(instance, null, new List<NonTerminal>() { occurrence })
            { }

            /// <summary>
            /// Constructs a non-terminal with a given operator and occurrences
            /// </summary>
            /// <param name="instance">LHS non-terminal</param>
            /// <param name="constructor">Operator</param>
            /// <param name="occurrences">RHS non-terminals</param>
            public Production(NonTerminal instance, SemgusTermType.Constructor constructor, IEnumerable<NonTerminal?> occurrences)
                : this(instance, constructor, occurrences.ToList())
            { }

            /// <inheritdoc />
            public override string ToString()
            {
                if (Constructor is null)
                {
                    return $"{Instance.Name} --> {Occurrences[0]!.Name}";
                }
                else if (Constructor.Children.Length == 0)
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
