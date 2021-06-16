namespace Semgus.Syntax {
    public class SemgusType {
        public string Name { get; }

        public SemgusType(string name) {
            Name = name;
        }

        public override string ToString() => Name;
    }
}