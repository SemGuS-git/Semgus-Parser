using System;

using Semgus.Syntax;

namespace Semgus.Parser.Example {
    class Program {
        static void Main(string[] args) {
            Main3(args);
        }

        public static void Main3(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Expects one argument: a Semgus file to parse");
            }

            var filename = args[0];

            SemgusParser parser = new(filename);
            parser.TryParse(out var problem);
            Console.Out.WriteLine(problem.ToString());

            var printer = new AstPrinter();

            // Print the AST
            Console.WriteLine(problem.GlobalEnvironment.PrettyPrint());
            Console.WriteLine(printer.PrettyPrint(problem));
        }
    }
}
