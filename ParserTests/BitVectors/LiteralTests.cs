using Semgus.Parser.Reader;
using Semgus.Parser.Reader.Converters;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Semgus.Parser.Tests.BitVectors
{
    /// <summary>
    /// Tests for bit vector literals
    /// </summary>
    public class LiteralTests
    {
        private static UInt64 BA2L(BitArray ba)
        {
            if (ba.Length > 64)
            {
                throw new ArgumentException("Array too big - only up to 64 supported, but got: " + ba.Length);
            }
            byte[] bs = new byte[8];
            ba.CopyTo(bs, 0);
            return BitConverter.ToUInt64(bs, 0);
        }

        [Theory]
        [InlineData("#b0", 0, 1)]
        [InlineData("#b1", 1, 1)]
        [InlineData("#b1111", 15, 4)]
        [InlineData("#b01010001", 81, 8)]
        [InlineData("#xFF", 255, 8)]
        [InlineData("#x1AAFF", 109311, 20)]
        [InlineData("#x0055", 85, 16)]
        [InlineData("#xDEADBEEF", 3735928559, 32)]
        [InlineData("#xDEADBEEFDEADBEEF", 16045690984833335023, 64)]
        public void ReadsLiterals(string text, ulong value, long size)
        {
            SemgusReader reader = new(text);
            var token = reader.Read();
            var bitLit = Assert.IsType<BitVectorToken>(token);
            Assert.Equal(size, bitLit.Value.Length);
            Assert.Equal(value, BA2L(bitLit.Value));
        }
    }
}
