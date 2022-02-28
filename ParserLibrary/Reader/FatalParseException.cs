using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Sexpr.Reader;

#nullable enable

namespace Semgus.Parser.Reader
{
    internal class FatalParseException : Exception
    {
        public SexprPosition? Position { get; }

        public FatalParseException(string msg, SexprPosition? position = default) : base(msg)
        {
            Position = position;
        }
    }
}
