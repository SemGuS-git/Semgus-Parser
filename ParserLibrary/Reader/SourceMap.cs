using Microsoft.Extensions.Logging;

using Semgus.Sexpr.Reader;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// Maps between parsed objects and their location in the source text
    /// </summary>
    internal class SourceMap : ISourceMap
    {
        /// <summary>
        /// Table holding mappings between objects and positions. Note that
        /// ConditionalWeakTable keys on reference, not value, and it doesn't
        /// prevent the keys from being garbage collected.
        /// </summary>
        private readonly ConditionalWeakTable<object, SexprPosition> _sourceMap;

        /// <summary>
        /// Logger for logging
        /// </summary>
        private readonly ILogger<SourceMap> _logger;

        /// <summary>
        /// Creates a new SourceMap with the given logger
        /// </summary>
        /// <param name="logger">Logger to use</param>
        public SourceMap(ILogger<SourceMap> logger)
        {
            _sourceMap = new ConditionalWeakTable<object, SexprPosition>();
            _logger = logger;
        }

        /// <summary>
        /// Gets or sets the position of a given object
        /// </summary>
        /// <param name="key">The object</param>
        /// <returns>The object's position</returns>
        /// <exception cref="InvalidOperationException">Thrown when a reference has a duplicate position added. This indicates a bug in the parser.</exception>
        public SexprPosition this[object key]
        {
            get
            {
                if (_sourceMap.TryGetValue(key, out var position))
                {
                    return position;
                }
                else
                {
                    _logger.LogDebug($"Source not found for object: {key}");
                    return SexprPosition.Default;
                }
            }

            set
            {   
                try
                {
                    _sourceMap.Add(key, value);
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException($"Attempt to add duplicate source key: {key}");
                }
            }
        }
    }
}
