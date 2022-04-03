using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Reader.Converters
{
    internal class IntConverter : AbstractConverter
    {
        public override bool CanConvert(Type from, Type to)
        {
            return from == typeof(NumeralToken) && (to == typeof(int) || to == typeof(long));
        }

        public override bool TryConvertImpl(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
            long value = ((NumeralToken)from).Value;
            if (tTo == typeof(int)) 
            { 
                to = (int)value;
            }
            else
            {
                to = value;
            } 
            return true;
        }
    }
}
