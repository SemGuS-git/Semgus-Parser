using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using FakeItEasy;
using Semgus.Model.Smt;
using Semgus.Sexpr.Reader;
using Semgus.Parser.Reader;
using Semgus.Model;
using Semgus.Parser.Commands;
using Microsoft.Extensions.Logging;

namespace Semgus.Parser.Tests.Issues
{
    /// <summary>
    /// https://github.com/SemGuS-git/Semgus-Parser/issues/46
    /// When a production in a synth-fun grammar block isn't found, the parser prints an error,
    /// but then also throws an exception and dumps a nasty stack trace.
    /// 
    /// Fix: throw FatalParseException instead of InvalidOperationException, because we actually
    /// have nice handling for it. Also fix up the logger scopes to give more information about where
    /// in the parsing phase the error occurred.
    /// </summary>
    public class Issue46
    {
        private ISmtContextProvider FakeSmtCtx(SmtContext ctx)
        {
            var fake = A.Fake<ISmtContextProvider>();
            A.CallTo(() => fake.Context).Returns(ctx);
            return fake;
        }

        [Fact]
        public void RejectsInvalidNullaryOp()
        {
            SmtIdentifier ntId = new("E");
            SmtSortIdentifier ttId = new("Term");
            SmtIdentifier opId = new("op-that-does-not-exist");
            SymbolToken opSymbol = new(opId.Symbol, SexprPosition.Default);

            SmtContext smtCtx = new();
            SemgusTermType tt = new(ttId);
            smtCtx.AddSortDeclaration(tt);

            List<(SmtIdentifier, SmtSortIdentifier)> ntMap = new()
            {
                (ntId, ttId)
            };

            List<(SmtIdentifier, SmtSortIdentifier, IList<SemgusToken>)> prods = new()
            {
                (ntId, ttId, new List<SemgusToken>() { opSymbol })
            };

            SynthFunCommand.GrammarForm gf = new(ntMap, prods);

            var converter = A.Fake<ISmtConverter>();
            A.CallTo(() => converter.TryConvert(opSymbol, out opId!)).Returns(true);

            SynthFunCommand sfc = new(
                A.Fake<ISemgusProblemHandler>(),
                FakeSmtCtx(smtCtx),
                A.Fake<ISemgusContextProvider>(),
                converter,
                A.Fake<ISourceMap>(),
                A.Fake<ISourceContextProvider>(),
                A.Fake<ILogger<SynthFunCommand>>());

            Assert.Throws<FatalParseException>(() => sfc.CreateGrammarFromForm(gf));
        }

        [Fact]
        public void RejectsInvalidArity()
        {
            SmtIdentifier ntId = new("E");
            SmtSortIdentifier ttId = new("Term");
            SmtIdentifier opId = new("op");
            SymbolToken opSymbol = new(opId.Symbol, SexprPosition.Default);
            SymbolToken ntSymbol = new(ntId.Symbol, SexprPosition.Default);
            ConsToken cons = new(opSymbol, new ConsToken(ntSymbol,
                                                         new NilToken(SexprPosition.Default),
                                                         SexprPosition.Default),
                                           SexprPosition.Default);
            IList<SmtIdentifier> consList = new List<SmtIdentifier>()
            {
                opId, ntId
            };

            SmtContext smtCtx = new();
            SemgusTermType tt = new(ttId);
            tt.AddConstructor(new(opId, tt, tt));
            smtCtx.AddSortDeclaration(tt);

            List<(SmtIdentifier, SmtSortIdentifier)> ntMap = new()
            {
                (ntId, ttId)
            };

            List<(SmtIdentifier, SmtSortIdentifier, IList<SemgusToken>)> prods = new()
            {
                (ntId, ttId, new List<SemgusToken>() { cons })
            };

            SynthFunCommand.GrammarForm gf = new(ntMap, prods);

            var converter = A.Fake<ISmtConverter>();
            A.CallTo(() => converter.TryConvert(cons, out consList!)).Returns(true);

            SynthFunCommand sfc = new(
                A.Fake<ISemgusProblemHandler>(),
                FakeSmtCtx(smtCtx),
                A.Fake<ISemgusContextProvider>(),
                converter,
                A.Fake<ISourceMap>(),
                A.Fake<ISourceContextProvider>(),
                A.Fake<ILogger<SynthFunCommand>>());

            Assert.Throws<FatalParseException>(() => sfc.CreateGrammarFromForm(gf));
        }

        [Fact]
        public void RejectsMalformed()
        {
            SmtIdentifier ntId = new("E");
            SmtSortIdentifier ttId = new("Term");
            SmtIdentifier opId = new("op");
            SymbolToken opSymbol = new(opId.Symbol, SexprPosition.Default);


            SmtContext smtCtx = new();
            SemgusTermType tt = new(ttId);
            tt.AddConstructor(new(opId, tt, tt));
            smtCtx.AddSortDeclaration(tt);

            List<(SmtIdentifier, SmtSortIdentifier)> ntMap = new()
            {
                (ntId, ttId)
            };

            List<(SmtIdentifier, SmtSortIdentifier, IList<SemgusToken>)> prods = new()
            {
                (ntId, ttId, new List<SemgusToken>() { opSymbol })
            };

            SynthFunCommand.GrammarForm gf = new(ntMap, prods);

            var converter = A.Fake<ISmtConverter>(); // Everything return false

            SynthFunCommand sfc = new(
                A.Fake<ISemgusProblemHandler>(),
                FakeSmtCtx(smtCtx),
                A.Fake<ISemgusContextProvider>(),
                converter,
                A.Fake<ISourceMap>(),
                A.Fake<ISourceContextProvider>(),
                A.Fake<ILogger<SynthFunCommand>>());

            Assert.Throws<FatalParseException>(() => sfc.CreateGrammarFromForm(gf));
        }
    }
}
