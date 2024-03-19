using FakeItEasy;

using Microsoft.Extensions.Logging;

using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;
using Semgus.Parser.Commands;
using Semgus.Parser.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Semgus.Parser.Tests.Issues
{
    /// <summary>
    /// https://github.com/SemGuS-git/Semgus-Parser/issues/43
    /// The constraint command evidently just dumps a nasty stack trace
    /// when the term is in an error state. This should be cleaned up to not be terrible.
    /// </summary>
    public class Issue43
    {
        private static ISmtConverter ConverterFor(object @in, SmtTerm @out)
        {
            var converter = A.Fake<ISmtConverter>();
            A.CallTo(() => converter.TryConvert(@in, out @out!)).Returns(true);
            return converter;
        }

        [Fact]
        public void ThrowsParseExceptionInErrorState()
        {
            SemgusToken testToken = new SymbolToken("test", default);
            SmtTerm testTerm = new ErrorTerm("test");

            ConstraintCommand cc = new(
                ConverterFor(testToken, testTerm),
                A.Fake<ISemgusProblemHandler>(),
                A.Fake<ISmtContextProvider>(),
                A.Fake<ISemgusContextProvider>(),
                A.Fake<ISmtScopeProvider>(),
                A.Fake<ISourceMap>(),
                A.Fake<IExtensionHandler>(),
                A.Fake<ILogger<ConstraintCommand>>());

            Assert.Throws<FatalParseException>(() => cc.Constraint(testToken));
        }

        [Fact]
        public void ThrowsParseExceptionForNonBool()
        {
            SemgusToken testToken = new SymbolToken("test", default);
            SmtTerm testTerm = new SmtStringLiteral(new SmtContext(), "test");

            ConstraintCommand cc = new(
                A.Fake<ISmtConverter>(),
                A.Fake<ISemgusProblemHandler>(),
                A.Fake<ISmtContextProvider>(),
                A.Fake<ISemgusContextProvider>(),
                A.Fake<ISmtScopeProvider>(),
                A.Fake<ISourceMap>(),
                A.Fake<IExtensionHandler>(),
                A.Fake<ILogger<ConstraintCommand>>());

            Assert.Throws<FatalParseException>(() => cc.Constraint(testToken));
        }

        [Fact]
        public void DoesNotThrowParseExceptionForBool()
        {
            SmtContext ctx = new();
            Assert.True(ctx.TryGetFunctionDeclaration(new("true"), out var truefn));
            Assert.True(truefn!.TryResolveRank(ctx, out var rank, default));

            SemgusToken testToken = new SymbolToken("test", default);
            SmtTerm testTerm = new SmtFunctionApplication(truefn, rank!, new List<SmtTerm>());

            ConstraintCommand cc = new(
                ConverterFor(@in: testToken, @out: testTerm),
                A.Fake<ISemgusProblemHandler>(),
                A.Fake<ISmtContextProvider>(),
                A.Fake<ISemgusContextProvider>(),
                A.Fake<ISmtScopeProvider>(),
                A.Fake<ISourceMap>(),
                A.Fake<IExtensionHandler>(),
                A.Fake<ILogger<ConstraintCommand>>());

            // Should not throw
            cc.Constraint(testToken);
        }
    }
}
