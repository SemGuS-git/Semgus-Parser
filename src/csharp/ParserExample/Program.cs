using System;
using Antlr4.Runtime;
using Semgus.Parser.Internal;

namespace Semgus.Parser.Example {
    class Program {
        static void Main(string[] args) {
            Main2(args);
        }
        
        // Parser / lexer demo
        static void Main1(string[] args) {
            if (args.Length != 1) {
                Console.Error.WriteLine("Expects one argument: a Semgus file to parse");
            }
            int errors = ExampleLibraryClass.TryParseGrammar(args[0]);
            Console.WriteLine($"{args[0]}: found {errors} errors");
        }
        
        // AST parser demo
        static void Main2(string[] args) {
            if (args.Length != 1) {
                Console.Error.WriteLine("Expects one argument: a Semgus file to parse");
            }
            var filename = args[0];

            SemgusLexer lexer = new SemgusLexer(new AntlrFileStream(filename));
            SemgusParser parser = new SemgusParser(new CommonTokenStream(lexer));
            
            var cst = parser.start();
            var ast = (StartNode)(new BuildAstVisitor().Visit(cst));
            
            var printer = new PrintAstVisitor();
            
            // Print the AST
            System.Console.WriteLine(printer.Visit(ast));
            System.Console.WriteLine("Types: " + string.Join(", ", printer.types));
            System.Console.WriteLine("Symbols: " + string.Join(", ", printer.symbols));
            
            System.Console.WriteLine("Done");
        }
    }
}
