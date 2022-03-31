using Microsoft.Extensions.Logging;

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
    internal class AnnotationConverter : AbstractConverter
    {
        private readonly ISmtConverter _converter;
        private readonly ISmtContextProvider _context;
        private readonly ILogger<AnnotationConverter> _logger;

        public AnnotationConverter(ISmtConverter converter, ISmtContextProvider context, ILogger<AnnotationConverter> logger)
        {
            _converter = converter;
            _context = context;
            _logger = logger;
        }

        public override bool CanConvert(Type from, Type to)
        {
            return to.IsAssignableTo(typeof(IList<SmtAttribute>))
                && (from.IsAssignableTo(typeof(IList<SemgusToken>))
                 || from.IsAssignableTo(typeof(IConsOrNil))); 
        }

        public override bool TryConvertImpl(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
            IList<SemgusToken> tokens;
            
            if (from is IList<SemgusToken> tokenList)
            {
                tokens = tokenList;
            }
            else if (from is IConsOrNil con)
            {
                tokens = DestructuringHelper.Listify(con);
            }
            else
            {
                to = default;
                return false;
            }

            List<SmtAttribute> attributes = new();

            int ix = 0;
            while (ix < tokens.Count)
            {
                if (!_converter.TryConvert(tokens[ix], out SmtKeyword? keyword))
                {
                    _logger.LogParseError("Expected a keyword in key position of annotation.", tokens[ix].Position);
                    break; // Just stop parsing and return what we have so far
                }

                // Check if the next index is a keyword. If not, then we need to parse out a value
                ix += 1;
                if (ix == tokens.Count || _converter.TryConvert(tokens[ix], out SmtKeyword? _))
                {
                    attributes.Add(new(keyword, new()));
                }
                else
                {
                    if (_converter.TryConvert(tokens[ix], out SmtAttributeValue? value))
                    {
                        attributes.Add(new(keyword, value));
                    }
                    else
                    {
                        _logger.LogParseError("Not a valid annotation attribute: " + tokens[ix], tokens[ix].Position);
                        break; // Just stop parsing and return what we have so far
                    }
                    ix += 1;
                }
            }

            to = attributes;
            return true;
        }
    }
}
