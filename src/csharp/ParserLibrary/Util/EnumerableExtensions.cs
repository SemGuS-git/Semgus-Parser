using System;
using System.Collections.Generic;

namespace Semgus.Parser.Util
{
    /// <summary>
    /// Extensions for working with enumerables
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Removes the first element from a sequence, returning the tail
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="first"></param>
        /// <returns></returns>
        public static IEnumerable<T> Pop<T>(this IEnumerable<T> sequence, out T first)
        {
            var enumerator = sequence.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                throw new InvalidOperationException("Pop called on an empty sequence");
            }
            first = enumerator.Current;

            return EnumerateRest(enumerator);
        }

        private static IEnumerable<T> EnumerateRest<T>(IEnumerator<T> rest)
        {
            while (rest.MoveNext())
            {
                yield return rest.Current;
            }
        }
    }
}
