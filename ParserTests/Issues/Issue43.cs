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
        [Fact]
        public void ThrowsParseExceptionInErrorState()
        {
            ConstraintCommand cc = new(
                A.Fake<ISemgusProblemHandler>(),
                A.Fake<ISmtContextProvider>(),
                A.Fake<ISemgusContextProvider>(),
                A.Fake<ISourceMap>(),
                A.Fake<ILogger<ConstraintCommand>>());

            Assert.Throws<FatalParseException>(() => cc.Constraint(new ErrorTerm("test")));
        }

        [Fact]
        public void ThrowsParseExceptionForNonBool()
        {
            ConstraintCommand cc = new(
                A.Fake<ISemgusProblemHandler>(),
                A.Fake<ISmtContextProvider>(),
                A.Fake<ISemgusContextProvider>(),
                A.Fake<ISourceMap>(),
                A.Fake<ILogger<ConstraintCommand>>());

            Assert.Throws<FatalParseException>(() => cc.Constraint(new SmtStringLiteral(new SmtContext(), "test")));
        }

        [Fact]
        public void DoesNotThrowParseExceptionForBool()
        {
            ConstraintCommand cc = new(
                A.Fake<ISemgusProblemHandler>(),
                A.Fake<ISmtContextProvider>(),
                A.Fake<ISemgusContextProvider>(),
                A.Fake<ISourceMap>(),
                A.Fake<ILogger<ConstraintCommand>>());

            SmtContext ctx = new();
            Assert.True(ctx.TryGetFunctionDeclaration(new("true"), out var truefn));
            Assert.True(truefn!.TryResolveRank(out var rank, default));

            // Should not throw
            cc.Constraint(new SmtFunctionApplication(truefn, rank!, new List<SmtTerm>()));
        }
    }
}
