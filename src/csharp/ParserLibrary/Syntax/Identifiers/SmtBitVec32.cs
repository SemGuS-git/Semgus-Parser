using System;

namespace Semgus.Syntax {
    public struct SmtBitVec32 : IEquatable<SmtBitVec32> {
        public readonly System.UInt32 value;

        public SmtBitVec32(uint value) {
            this.value = value;
        }

        public override bool Equals(object obj) => obj is SmtBitVec32 vec && Equals(vec);
        public bool Equals(SmtBitVec32 other) => value == other.value;

        public override int GetHashCode() => value.GetHashCode();

        public static bool operator ==(SmtBitVec32 left, SmtBitVec32 right) => left.Equals(right);
        public static bool operator !=(SmtBitVec32 left, SmtBitVec32 right) => !(left == right);

        public override string ToString() => "#x" + Convert.ToString(value, 16).PadLeft(8, '0');

        public static SmtBitVec32 Parse(string str) {
            uint value;
            if (str.StartsWith("#x")) {
                value = Convert.ToUInt32(str.Substring(2), 16);
            } else if (str.StartsWith("#b")) {
                value = Convert.ToUInt32(str.Substring(2), 2);
            } else {
                throw new ArgumentException();
            }
            return new SmtBitVec32(value);
        }
    }
}