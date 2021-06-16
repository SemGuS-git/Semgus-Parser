using System;
using System.Collections.Generic;

namespace Semgus.Syntax {
    /// <summary>
    /// Unique identifier for a nonterminal
    /// </summary>
    public class Nonterminal : IEquatable<Nonterminal> {
        public string Name { get; }

        public Nonterminal(string name) {
            Name = name;
        }

        public override string ToString() => Name;

        public override bool Equals(object obj) {
            return Equals(obj as Nonterminal);
        }

        public bool Equals(Nonterminal other) {
            return other != null &&
                   Name == other.Name;
        }

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        public static bool operator ==(Nonterminal left, Nonterminal right) {
            return EqualityComparer<Nonterminal>.Default.Equals(left, right);
        }

        public static bool operator !=(Nonterminal left, Nonterminal right) {
            return !(left == right);
        }
    }
}