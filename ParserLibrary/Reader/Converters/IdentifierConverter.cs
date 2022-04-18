using Microsoft.Extensions.Logging;

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
        private readonly ISmtConverter _converter;
        private readonly ILogger<IdentifierConverter> _logger;

        public IdentifierConverter(ISmtConverter converter, ILogger<IdentifierConverter> logger)
        {
            _converter = converter;
            _logger = logger;
        }

        public override bool CanConvert(Type from, Type to)
        {
            return to == typeof(SmtIdentifier) &&
                (from.IsAssignableFrom(typeof(SymbolToken))
                || from.IsAssignableFrom(typeof(ConsToken)));
        }

        public override bool TryConvertImpl(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
            if (tFrom.IsAssignableFrom(typeof(SymbolToken)))
            {
                to = new SmtIdentifier(((SymbolToken)from).Name);
                return true;
            }
            else if (tFrom.IsAssignableFrom(typeof(ConsToken)))
            {
                if (_converter.TryConvert(from, out IndexedIdentifierModel? iim))
                {
                    var indices = iim.Indices.Select(i =>
                    {
                        if (i is SymbolToken symb)
                        {
                            return new SmtIdentifier.Index(symb.Name);
                        }
                        else if (i is NumeralToken num)
                        {
                            return new SmtIdentifier.Index(num.Value);
                        }
                        else
                        {
                            throw _logger.LogParseErrorAndThrow("Invalid identifier index. Only symbols and numerals allowed, but got: " + i.GetType().Name, i.Position);
                        }
                    });
                    to = new SmtIdentifier(iim.Primary.Symbol, indices.ToArray());
                    return true;
                }
            }
            to = default;
            return false;
        }

        private record IndexedIdentifierModel([Exactly("_")] SmtIdentifier _, SmtIdentifier Primary, [Rest] IList<SemgusToken> Indices);
    }
}
