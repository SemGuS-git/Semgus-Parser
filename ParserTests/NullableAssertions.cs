using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Semgus.Parser.Tests
{
    /// <summary>
    /// This class exists because Xunit's assertions don't have the nullable attributes
    /// </summary>
    internal static class NullableAssertions
    {
        /// <summary>
        /// Asserts that the given object is not null
        /// </summary>
        /// <param name="thing">Thing to check that it is not null</param>
        public static void AssertNotNull([NotNull] object? thing)
        {
            Assert.NotNull(thing);
            if (thing is null)
            {
                throw new InvalidProgramException("Got somewhere impossible.");
            }
        }

        /// <summary>
        /// Asserts that the given condition is true
        /// </summary>
        /// <param name="condition">The condition to check</param>
        public static void AssertTrue([DoesNotReturnIf(false)] bool condition)
        {
            Assert.True(condition);
        }

        /// <summary>
        /// Asserts that the given condition is false
        /// </summary>
        /// <param name="condition">The condition to check</param>
        public static void AssertFalse([DoesNotReturnIf(true)] bool condition)
        {
            Assert.False(condition);
        }
    }
}
