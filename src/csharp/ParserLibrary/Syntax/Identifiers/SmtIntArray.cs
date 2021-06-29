using System.Collections.Generic;
using System.Linq;
using System;

namespace Semgus.Syntax {
    public class SmtIntArray : IEquatable<SmtIntArray> {
        private readonly Dictionary<int, int> _map;

        public SmtIntArray(IReadOnlyList<int> data) {
            var map = new Dictionary<int, int>();
            for (int i = 0; i < data.Count; i++) map[i] = data[i];
            _map = map;
        }

        public SmtIntArray() {
            _map = new Dictionary<int, int>();
        }

        // Private constructor because this wraps its input argument without cloning
        private SmtIntArray(Dictionary<int, int> map) {
            _map = map;
        }

        public SmtIntArray Store(int idx, int value) {
            var copy = new Dictionary<int, int>(_map);
            copy[idx] = value;
            return new SmtIntArray(copy);
        }

        public int Select(int idx) {
            if (_map.TryGetValue(idx, out var value)) {
                return value;
            } else {
#if RETURN_ZERO_ON_KEY_MISS
                _map[idx] = 0;
                return 0;
#else
                throw new KeyNotFoundException();
#endif
            }
        }

        public override bool Equals(object obj) => Equals(obj as SmtIntArray);

        public bool Equals(SmtIntArray other) =>
            (!(other is null)) &&
            (_map.Count == other._map.Count) &&
            other._map.All(kvp => _map.TryGetValue(kvp.Key, out var v) && kvp.Value == v);

        public override int GetHashCode() {
            var hc = 0;
            foreach (var kvp in _map) {
                hc = HashCode.Combine(hc, kvp.Key, kvp.Value);
            }
            return hc;
        }

        public static bool operator ==(SmtIntArray left, SmtIntArray right) => (left is null && right is null) || left.Equals(right);

        public static bool operator !=(SmtIntArray left, SmtIntArray right) => !(left == right);
    }
}
