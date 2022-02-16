using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Parser.Reader.Converters
{
    internal class IdentityConverter : AbstractConverter
    {
        public override bool CanConvert(Type from, Type to) => from.IsAssignableTo(to);

        public override bool TryConvertImpl(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
            to = from;
            return true;
        }
    }
}
