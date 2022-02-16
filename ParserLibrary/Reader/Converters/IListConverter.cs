using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Parser.Reader.Converters
{
    internal class IListConverter : AbstractConverter
    {
        private readonly SmtConverter _converter;

        public IListConverter(SmtConverter converter)
        {
            _converter = converter;
        }

        public override bool CanConvert(Type from, Type to)
        {
            return from.IsAssignableTo(typeof(IConsOrNil)) && to.IsGenericType && to.GetGenericTypeDefinition() == typeof(IList<>);
        }

        public override bool TryConvertImpl(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
            var resolvedType = tTo.GenericTypeArguments[0];
            var list = new GenericList(resolvedType);
            IList<SemgusToken> tokens = DestructuringHelper.Listify((IConsOrNil)from);
            foreach (var token in tokens)
            {
                if (_converter.TryConvert(token.GetType(), resolvedType, token, out var converted))
                {
                    list.Add(converted);
                }
                else
                {
                    Console.WriteLine($"Cannot convert {token.GetType().Name} to {resolvedType.Name} in list");
                    to = default;
                    return false;
                }
            }
            to = list.List;
            return true;
        }
    }
}
