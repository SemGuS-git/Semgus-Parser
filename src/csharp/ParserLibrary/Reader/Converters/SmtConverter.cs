using Microsoft.Extensions.DependencyInjection;

using Semgus.Model.Smt;
using Semgus.Parser.Reader.Converters;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Parser.Reader
{
    public class SmtConverter
    {
        private readonly IServiceProvider _provider;

        private IList<AbstractConverter>? _converters = null;

        private IList<AbstractConverter> Converters
        {
            get
            {
                if (_converters == null)
                {
                    _converters = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .Where(t => t.IsAssignableTo(typeof(AbstractConverter)))
                            .Where(t => !t.IsAbstract)
                            .Select(t => (AbstractConverter)ActivatorUtilities.CreateInstance(_provider, t)!)
                            .Where(c => c != null)
                            .OrderBy(c => c.Priority)
                            .ToList();
                }
                return _converters;
            }
        }

        public SmtConverter(IServiceProvider provider)
        {
            _provider = provider;
        }


        public bool TryConvert(Type tFrom, Type tTo, object from, [NotNullWhen(true)] out object? to)
        {
            foreach (var converter in Converters)
            {
                if (converter.CanConvert(tFrom, tTo) && converter.TryConvert(tFrom, tTo, from, out to))
                {
                    return true;
                }
            }

            to = default;
            return false;
        }

        public bool TryConvert<TTarget>(object from, [NotNullWhen(true)] out TTarget? to)
        {
            if (TryConvert(from.GetType(), typeof(TTarget), from, out var output))
            {
                to = (TTarget)output;
                return true;
            }
            else
            {
                to = default;
                return false;
            }
        }
    }
}
