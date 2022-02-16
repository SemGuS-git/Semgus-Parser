using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Parser.Reader.Converters
{
    internal class KeywordConverter : AbstractConverter
    {
        public override bool CanConvert(Type from, Type to)
        {
            return to == typeof(SmtKeyword) && from.IsAssignableTo(typeof(KeywordToken));
        }

        public override bool TryConvertImpl(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
            to = new SmtKeyword(((KeywordToken)from).Name);
            return true;
        }
    }
}
