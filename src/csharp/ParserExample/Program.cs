using System;

namespace Semgus.Parser.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Expects one argument: a Semgus file to parse");
            }
            int errors = ExampleLibraryClass.TryParseGrammar(args[0]);
            Console.WriteLine($"{args[0]}: found {errors} errors");
        }
    }
}
