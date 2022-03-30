using Semgus.Model;
using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Json
{
    internal class SynthFunEvent : ParseEvent
    {
        public SmtIdentifier Name { get; }
        public SmtIdentifier TermType { get; }
        public GrammarModel Grammar { get; }

        public SynthFunEvent(SemgusSynthFun ssf) : base("synth-fun", "semgus")
        {
            Name = ssf.Relation.Name;
            TermType = ssf.Rank.ReturnSort.Name.Name;
            Grammar = new GrammarModel(ssf.Grammar.NonTerminals.Select(n => new NonTerminalDeclarationModel(n.Name, n.Sort.Name.Name)),
                                       ssf.Grammar.Productions.Select(p => new ProductionModel(p.Instance.Name,
                                                                                               p.Constructor?.Operator,
                                                                                               p.Occurrences.Select(o => o?.Name))));
        }

        public record NonTerminalDeclarationModel(SmtIdentifier Name, SmtIdentifier TermType);
        public record ProductionModel(SmtIdentifier Instance, SmtIdentifier? Operator, IEnumerable<SmtIdentifier?> Occurrences);
        public record GrammarModel(IEnumerable<NonTerminalDeclarationModel> NonTerminals, IEnumerable<ProductionModel> Productions);
    }
}
