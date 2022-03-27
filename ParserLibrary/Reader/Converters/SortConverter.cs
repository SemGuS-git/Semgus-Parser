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
    internal class SortConverter : AbstractConverter
    {
        private readonly ISmtConverter _converter;
        private readonly ISmtContextProvider _context;

        public SortConverter(ISmtConverter converter, ISmtContextProvider context)
        {
            _converter = converter;
            _context = context;
        }

        public override bool CanConvert(Type from, Type to)
        {
            return to == typeof(SmtSort); // Lot of options for this one... && from.IsAssignableFrom(typeof(SymbolToken));
        }

        public override bool TryConvertImpl(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
            // Sort type 1: just an identifier
            if (_converter.TryConvert(from, out SmtIdentifier? id))
            {
                to = _context.Context.GetSortDeclaration(id);
                return true;
            }

            // Sort type 2: identifier application (parameterized sorts)
            if (_converter.TryConvert(from, out ParameterizedSortForm? sortform))
            {
                var parameterized = _context.Context.GetSortDeclaration(sortform.Identifier);
                to = _context.Context.ResolveParameterizedSort(parameterized, sortform.Parameters);
                return true;
            }

            Console.WriteLine("Unable to resolve sort: " + from.ToString());

            to = default;
            return false;
        }

        private record ParameterizedSortForm(SmtIdentifier Identifier, [Rest] IList<SmtSort> Parameters) { }
    }
}
