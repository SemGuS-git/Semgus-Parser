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

namespace Semgus.Parser.Reader.Converters
{
    /// <summary>
    /// Interface for converting between S-expression tokens and SMT objects
    /// </summary>
    public class SmtConverter : ISmtConverter
    {
        /// <summary>
        /// Service provider that converters will be taken from
        /// </summary>
        private readonly IServiceProvider _provider;

        /// <summary>
        /// Cached list of converters
        /// </summary>
        private IList<AbstractConverter>? _converters = null;

        /// <summary>
        /// List of all active converters that will be tried. Loads lazily
        /// </summary>
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

        /// <summary>
        /// Constructs a new SmtConverter, where converters will be pulled from the given service provider
        /// </summary>
        /// <param name="provider">Service provider in which to look for converters</param>
        public SmtConverter(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Converts an object of type tFrom to an object of type tTo. Usually, from is an 
        /// S-expression token and to is an SMT object
        /// </summary>
        /// <param name="tFrom">Type to convert from</param>
        /// <param name="tTo">Type to convert to</param>
        /// <param name="from">Object to convert from</param>
        /// <param name="to">Conversion result</param>
        /// <returns>True if successfully converted</returns>
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

        /// <summary>
        /// Converts an object from to an object to. Usually, from is an 
        /// S-expression token and to is an SMT object. The to and from types are inferred
        /// from the generic parameters.
        /// </summary>
        /// <param name="from">Object to convert from</param>
        /// <param name="to">Conversion result</param>
        /// <returns>True if successfully converted</returns>
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
