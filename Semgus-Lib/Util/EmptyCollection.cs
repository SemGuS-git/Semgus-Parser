using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Util {
    public class EmptyCollection<T> : IReadOnlyList<T> {
        public T this[int index] => throw new IndexOutOfRangeException();

        public static EmptyCollection<T> Instance { get; } = new EmptyCollection<T>();

        public int Count => 0;

        public IEnumerator<T> GetEnumerator() => Enumerator.Instance;

        IEnumerator IEnumerable.GetEnumerator() => Enumerator.Instance;

        private class Enumerator : IEnumerator<T> {
            public static Enumerator Instance { get; } = new Enumerator();

            public T Current => throw new InvalidOperationException();

            object IEnumerator.Current => throw new InvalidOperationException();

            public void Dispose() { }

            public bool MoveNext() => false;

            public void Reset() => throw new NotSupportedException();
        }
    }
}
