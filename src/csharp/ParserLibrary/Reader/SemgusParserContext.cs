using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Sexpr.Reader;

namespace Semgus.Parser.Reader
{
    public record SemgusParserContext(SexprPosition Position)
    {
        public SemgusParserContext(SemgusToken sexpr) : this(sexpr.Position) { }
        public SexprPosition Start => Position;

        public static implicit operator SemgusParserContext(SexprPosition pos) => new SemgusParserContext(pos);
    }
}
