using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Parser.Reader.Converters
{
    internal class ConstructorConverter : AbstractConverter
    {
        public override int Priority => 1000;

        private readonly DestructuringHelper _destructuringHelper;

        public ConstructorConverter(DestructuringHelper destructuringHelper)
        {
            _destructuringHelper = destructuringHelper;
        }

        public override bool CanConvert(Type from, Type to)
        {
            return from.IsAssignableTo(typeof(IConsOrNil)) && !to.IsAbstract; // All we know so far
        }

        public override bool TryConvertImpl(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
            var list = DestructuringHelper.Listify((IConsOrNil)from);
            foreach (var cInfo in tTo.GetConstructors())
            {
                var cParams = cInfo.GetParameters();
                if (_destructuringHelper.TryDestructure(cParams, (IConsOrNil)from, out var parameters))
                {
                    to = cInfo.Invoke(parameters);
                    return true;
                }
            }
            to = default;
            return false;
        }
    }
}
