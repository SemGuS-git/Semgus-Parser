using Microsoft.Extensions.Logging;

using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Parser.Reader.Converters
{
    internal class AttributeValueConverter : AbstractConverter
    {
        private readonly SmtConverter _converter;
        private readonly ISmtContextProvider _context;
        private readonly ILogger<AttributeValueConverter> _logger;

        public override int Priority => -10;

        public AttributeValueConverter(SmtConverter converter, ISmtContextProvider context, ILogger<AttributeValueConverter> logger)
        {
            _converter = converter;
            _context = context;
            _logger = logger;
        }

        public override bool CanConvert(Type from, Type to)
        {
            return to.IsAssignableTo(typeof(SmtAttributeValue));
        }

        public override bool TryConvertImpl(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
            if (_converter.TryConvert(from, out SmtLiteral? literal))
            {
                to = new SmtAttributeValue(literal);
                return true;
            }
            else if (_converter.TryConvert(from, out SmtIdentifier? id))
            {
                to = new SmtAttributeValue(id);
                return true;
            }
            else if (_converter.TryConvert(from, out SmtKeyword? keyword))
            {
                to = new SmtAttributeValue(keyword);
                return true;
            }
            else if (_converter.TryConvert(from, out IList<SmtAttributeValue>? list))
            {
                to = new SmtAttributeValue(list);
                return true;
            }

            to = default;
            return false;
        }

        private record ParameterizedSortForm(SmtIdentifier Identifier, [Rest] IList<SmtSort> Parameters) { }
    }
}
