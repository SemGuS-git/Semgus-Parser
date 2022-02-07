using System;
using System.IO;

using Semgus.Syntax;

namespace Semgus.Parser.Example {
    class Program {
        static void Main(string[] args) {
            Main4(args);
        }

        public static void Main4(string[] args)
        {
            string data = @"
            (set-info :test)

            (synth-fun QQQ ((x Int)) Bool)
            (declare-term-types ((A 0) (B 0))
               (((q) (x (x1 Int)))
                ((qwer (qwer1 Bool) (qwer2 A)) (asdf (asdf1 B)))))
            (define-funs-rec ((f ((x Int) (y Bool)) Bool)) ())

            (constraint (+ 1 2))
            (constraint true)
            (constraint (= 5 6))

";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(data);
            writer.Flush();
            stream.Position = 0;
            SemgusParser2 parser = new(stream, "string");
            parser.TryParse(new ExampleSemgusProblemHandler());
        }

        public static void Main3(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Expects one argument: a Semgus file to parse");
            }

            var filename = args[0];

            SemgusParser parser = new(filename);
            parser.TryParse(out var problem, out int errCount);
            if (problem is not null)
            {
                Console.Out.WriteLine(problem.ToString());

                var printer = new AstPrinter();

                // Print the AST
                Console.WriteLine(problem.GlobalEnvironment.PrettyPrint());
                Console.WriteLine(printer.PrettyPrint(problem));
            }
            else
            {
                Console.WriteLine($"Failed to parse file. Encountered {errCount} error{(errCount != 1 ? "s" : "")}");
            }
        }
    }
}
