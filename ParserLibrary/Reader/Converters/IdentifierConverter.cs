using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Reader.Converters
{
    internal class IdentifierConverter : AbstractConverter
    {
        // TODO: handle indexed identifiers
        public override bool CanConvert(Type from, Type to)
        {
            return to == typeof(SmtIdentifier) && from.IsAssignableFrom(typeof(SymbolToken));
        }

        public override bool TryConvertImpl(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
            to = new SmtIdentifier(((SymbolToken)from).Name);
            return true;
        }
    }
}
