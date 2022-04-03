using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Reader.Converters
{
    internal class SortConverter : AbstractConverter
    {
        private readonly ISmtConverter _converter;

        public SortConverter(ISmtConverter converter)
        {
            _converter = converter;
        }

        public override bool CanConvert(Type from, Type to)
        {
            if (to == typeof(SmtSort))
            {
                throw new InvalidOperationException("SmtSort is not a valid conversion target.");
            }
            return to == typeof(SmtSortIdentifier); // Lot of options for this one... && from.IsAssignableFrom(typeof(SymbolToken));
        }

        public override bool TryConvertImpl(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
            // Sort type 1: just an identifier
            if (_converter.TryConvert(from, out SmtIdentifier? id))
            {
                to = new SmtSortIdentifier(id);
                return true;
            }

            // Sort type 2: identifier application (parameterized sorts)
            if (_converter.TryConvert(from, out ParameterizedSortForm? sortform))
            {
                to = new SmtSortIdentifier(sortform.Identifier, sortform.Parameters.ToArray());
                return true;
            }

            to = default;
            return false;
        }

        private record ParameterizedSortForm(SmtIdentifier Identifier, [Rest] IList<SmtSortIdentifier> Parameters) { }
    }
}
