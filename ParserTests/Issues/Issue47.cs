using FakeItEasy;

using Microsoft.Extensions.Logging;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Parser;
using Semgus.Parser.Commands;
using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Semgus.Parser.Tests.Issues
{
    /// <summary>
    /// https://github.com/SemGuS-git/Semgus-Parser/issues/47
    /// "No-op" productions in synth-fun grammar blocks are evidently not supported.
    /// For example, the Start -> A production in the following example:
    ///
    /// (synth-fun max2 () E ; synth-fun block: defines term max2 to synthesize, of term-type E
    ///  (((Start E) (A E)) ; Nonterminals of subgrammar
    ///  ((Start E (A ($+ A A))) ; Productions of Start: Start -> A and Start -> A + A
    ///    (A E ($1 $x))))) ; Productions of A: A -> 1 and A -> x
    ///
    /// This is interesting because there is no term constructor corresponding to these productions,
    /// so we'll see how it can be handled.
    /// </summary>
    public class Issue47
    {
        [Fact]
        public void HandlesNtToNtProductions()
        {
            SmtIdentifier ntId = new("nt");
            SmtSortIdentifier ttId = new("Term");
            SymbolToken ntSymbol = new(ntId.Symbol, SexprPosition.Default);

            SmtContext smtCtx = new();
            SemgusTermType tt = new(ttId);
            smtCtx.AddSortDeclaration(tt);

            List<(SmtIdentifier, SmtSort)> ntMap = new()
            {
                (ntId, tt)
            };

            List<(SmtIdentifier, SmtSort, IList<SemgusToken>)> prods = new()
            {
                (ntId, tt, new List<SemgusToken>() { ntSymbol })
            };

            SynthFunCommand.GrammarForm gf = new(ntMap, prods);

            var converter = A.Fake<ISmtConverter>();
            A.CallTo(() => converter.TryConvert(ntSymbol, out ntId!)).Returns(true);

            SynthFunCommand sfc = new(
                A.Fake<ISemgusProblemHandler>(),
                A.Fake<ISmtContextProvider>(),
                A.Fake<ISemgusContextProvider>(),
                converter,
                A.Fake<ISourceMap>(),
                A.Fake<ILogger<SynthFunCommand>>());

            SemgusGrammar grammar = sfc.CreateGrammarFromForm(gf);

            Assert.Contains(grammar.Productions,
                p => p.Instance.Name == ntId
                  && p.Constructor is null
                  && p.Occurrences is not null
                  && p.Occurrences.Count() == 1
                  && p.Occurrences[0] is not null
                  && p.Occurrences[0]!.Name == ntId);
        }

        [Fact]
        public void HandlesNtToOtherNtProductions()
        {
            SmtIdentifier nt1Id = new("E");
            SmtIdentifier nt2Id = new("A");
            SmtSortIdentifier ttId = new("Term");
            SymbolToken nt2Symbol = new(nt2Id.Symbol, SexprPosition.Default);

            SmtContext smtCtx = new();
            SemgusTermType tt = new(ttId);
            smtCtx.AddSortDeclaration(tt);

            List<(SmtIdentifier, SmtSort)> ntMap = new()
            {
                (nt1Id, tt),
                (nt2Id, tt)
            };

            List<(SmtIdentifier, SmtSort, IList<SemgusToken>)> prods = new()
            {
                (nt1Id, tt, new List<SemgusToken>() { nt2Symbol })
            };

            SynthFunCommand.GrammarForm gf = new(ntMap, prods);

            var converter = A.Fake<ISmtConverter>();
            A.CallTo(() => converter.TryConvert(nt2Symbol, out nt2Id!)).Returns(true);

            SynthFunCommand sfc = new(
                A.Fake<ISemgusProblemHandler>(),
                A.Fake<ISmtContextProvider>(),
                A.Fake<ISemgusContextProvider>(),
                converter,
                A.Fake<ISourceMap>(),
                A.Fake<ILogger<SynthFunCommand>>());

            SemgusGrammar grammar = sfc.CreateGrammarFromForm(gf);

            Assert.Contains(grammar.Productions,
                p => p.Instance.Name == nt1Id
                  && p.Constructor is null
                  && p.Occurrences is not null
                  && p.Occurrences.Count() == 1
                  && p.Occurrences[0] is not null
                  && p.Occurrences[0]!.Name == nt2Id);
        }

        [Fact]
        public void RejectsMismatchedTermTypesForNtToNtProductions()
        {
            SmtIdentifier nt1Id = new("E");
            SmtIdentifier nt2Id = new("A");
            SmtSortIdentifier tt1Id = new("Term1");
            SmtSortIdentifier tt2Id = new("Term2");
            SymbolToken nt2Symbol = new(nt2Id.Symbol, SexprPosition.Default);

            SmtContext smtCtx = new();
            SemgusTermType tt1 = new(tt1Id);
            SemgusTermType tt2 = new(tt2Id);
            smtCtx.AddSortDeclaration(tt1);
            smtCtx.AddSortDeclaration(tt2);

            List<(SmtIdentifier, SmtSort)> ntMap = new()
            {
                (nt1Id, tt1),
                (nt2Id, tt2)
            };

            List<(SmtIdentifier, SmtSort, IList<SemgusToken>)> prods = new()
            {
                (nt1Id, tt1, new List<SemgusToken>() { nt2Symbol })
            };

            SynthFunCommand.GrammarForm gf = new(ntMap, prods);

            var converter = A.Fake<ISmtConverter>();
            A.CallTo(() => converter.TryConvert(nt2Symbol, out nt2Id!)).Returns(true);

            SynthFunCommand sfc = new(
                A.Fake<ISemgusProblemHandler>(),
                A.Fake<ISmtContextProvider>(),
                A.Fake<ISemgusContextProvider>(),
                converter,
                A.Fake<ISourceMap>(),
                A.Fake<ILogger<SynthFunCommand>>());

            Assert.Throws<FatalParseException>(() => sfc.CreateGrammarFromForm(gf));
        }

        [Fact]
        public void OverridesNullaryConstructors()
        {
            SmtIdentifier ntId = new("nt");
            SmtSortIdentifier ttId = new("Term");
            SymbolToken ntSymbol = new(ntId.Symbol, SexprPosition.Default);

            SmtContext smtCtx = new();
            SemgusTermType tt = new(ttId);
            tt.AddConstructor(new(ntId));
            smtCtx.AddSortDeclaration(tt);

            List<(SmtIdentifier, SmtSort)> ntMap = new()
            {
                (ntId, tt)
            };

            List<(SmtIdentifier, SmtSort, IList<SemgusToken>)> prods = new()
            {
                (ntId, tt, new List<SemgusToken>() { ntSymbol })
            };

            SynthFunCommand.GrammarForm gf = new(ntMap, prods);

            var converter = A.Fake<ISmtConverter>();
            A.CallTo(() => converter.TryConvert(ntSymbol, out ntId!)).Returns(true);

            SynthFunCommand sfc = new(
                A.Fake<ISemgusProblemHandler>(),
                A.Fake<ISmtContextProvider>(),
                A.Fake<ISemgusContextProvider>(),
                converter,
                A.Fake<ISourceMap>(),
                A.Fake<ILogger<SynthFunCommand>>());

            SemgusGrammar grammar = sfc.CreateGrammarFromForm(gf);

            Assert.Single(grammar.Productions);
            Assert.Contains(grammar.Productions,
                p => p.Instance.Name == ntId
                  && p.Constructor is null
                  && p.Occurrences is not null
                  && p.Occurrences.Count() == 1
                  && p.Occurrences[0] is not null
                  && p.Occurrences[0]!.Name == ntId);
        }

        [Fact]
        public void RejectsInvalidNts()
        {
            SmtIdentifier nt1Id = new("E");
            SmtIdentifier nt2Id = new("A");
            SmtIdentifier bogusId = new("Bogus");
            SmtSortIdentifier ttId = new("Term");
            SymbolToken nt2Symbol = new(bogusId.Symbol, SexprPosition.Default);

            SmtContext smtCtx = new();
            SemgusTermType tt = new(ttId);
            smtCtx.AddSortDeclaration(tt);

            List<(SmtIdentifier, SmtSort)> ntMap = new()
            {
                (nt1Id, tt),
                (nt2Id, tt)
            };

            List<(SmtIdentifier, SmtSort, IList<SemgusToken>)> prods = new()
            {
                (nt1Id, tt, new List<SemgusToken>() { nt2Symbol })
            };

            SynthFunCommand.GrammarForm gf = new(ntMap, prods);

            var converter = A.Fake<ISmtConverter>();
            A.CallTo(() => converter.TryConvert(nt2Symbol, out bogusId!)).Returns(true);

            SynthFunCommand sfc = new(
                A.Fake<ISemgusProblemHandler>(),
                A.Fake<ISmtContextProvider>(),
                A.Fake<ISemgusContextProvider>(),
                converter,
                A.Fake<ISourceMap>(),
                A.Fake<ILogger<SynthFunCommand>>());

            Assert.Throws<FatalParseException>(() => sfc.CreateGrammarFromForm(gf));
        }
    }
}
