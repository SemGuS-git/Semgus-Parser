namespace Semgus.Syntax {
    /// <summary>
    /// A function that is assumed to be defined in some external library or theory.
    /// </summary>
    public class LibraryFunction {
        public string Name { get; }
        
        public LibraryFunction(string name) {
            this.Name = name;
        }
        
        public override string ToString() => Name;
    }
}