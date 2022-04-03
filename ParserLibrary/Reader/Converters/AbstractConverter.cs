using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Reader.Converters
{
    internal abstract class AbstractConverter
    {
        public abstract bool CanConvert(Type from, Type to);

        public virtual int Priority { get => 0; }

        public bool TryConvert(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
            if (CanConvert(tFrom, tTo))
            {
                return TryConvertImpl(tFrom, tTo, from, out to);
            }
            else
            {
                to = default;
                return false;
            }
        }

        public abstract bool TryConvertImpl(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to);
    }
}
