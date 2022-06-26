using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

using Semgus.Parser.Json;
using Semgus.Parser.Sexpr;
using Semgus.Parser.Verifier;

namespace Semgus.Parser
{
    /// <summary>
    /// Main class for running the parser
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Output format options
        /// </summary>
        public enum OutputFormat
        {
            Json,
            Sexpr,
            Verify
        }

        /// <summary>
        /// Processing mode options.
        /// Streaming should provide a stream of independent "events" (or "commands") that can be
        /// split and concatenated as necessary, while batch mode should produce a standalone file.
        /// </summary>
        public enum ProcessingMode
        {
            Stream,
            Batch
        }

        /// <summary>
        /// Main entry point of the parser application
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>0 if parsing succeeded without errors, 2 if parsing fatally failed (exited early), and 7 if any errors reported.</returns>
        public static int Main(string[] args)
        {
            var modeOption = new Option<ProcessingMode>(
                name: "--mode",
                getDefaultValue: () => ProcessingMode.Stream,
                description: "Mode to process inputs in."
                );

            var formatOption = new Option<OutputFormat>(
                name: "--format",
                getDefaultValue: () => OutputFormat.Verify,
                description: "Output format."
                );

            var testOption = new Option<bool>(
                name: "--test",
                description: "Invokes the parser on a test string."
                );
            testOption.IsHidden = true;

            var outputOption = new Option<string>(
                name: "--output",
                getDefaultValue: () => "-",
                description: "File to write output to. Outputs to standard out if not specified or `-`."
                );

            var inputArguments = new Argument<string[]>(
                name: "inputs",
                getDefaultValue: () => new string[] { "-" },
                "Files to read SemGuS problems from, or `-` to read from standard input."
                );

            var rootCommand = new RootCommand("Parses SemGuS problem files.")
            {
                modeOption,
                formatOption,
                testOption,
                outputOption,
                inputArguments
            };

            rootCommand.SetHandler(
                handle: (ProcessingMode mode, OutputFormat format,
                         bool test, string output, string[] inputs,
                         InvocationContext ctx)
                            => ctx.ExitCode = Execute(mode, format, test, output, inputs), 
                modeOption,
                formatOption,
                testOption,
                outputOption,
                inputArguments);

            return rootCommand.Invoke(args);
        }

        /// <summary>
        /// Runs the parser with the given options
        /// </summary>
        /// <param name="mode">The processing mode to use</param>
        /// <param name="format">The desired output format</param>
        /// <param name="test">Whether or not to pull from the internal test string instead of input files</param>
        /// <param name="output">The output file, or "-" for standard output</param>
        /// <param name="inputs">Enumerable of input file names</param>
        /// <returns>Exit code from parsing</returns>
        private static int Execute(ProcessingMode mode, OutputFormat format, bool test, string output, IEnumerable<string> inputs)
        {
            using var writerDisposable = GetOutputWriter(output, out var writer);
            using var handlerDisposable = GetHandler(writer, mode, format, out var handler);
            int errCount = 0;
            foreach (var input in inputs)
            {
                using SemgusParser parser = GetParser(input, test, out var friendlyName);
                if (!parser.TryParse(handler, out errCount))
                {
                    Console.Error.WriteLine("error: fatal error reported while parsing " + friendlyName);
                    return 2;
                }
            }

            if (errCount == 0)
            {
                return 0;
            }
            else
            {
                Console.Error.WriteLine($"Encountered {errCount} fatal error{(errCount == 1 ? "" : "s")} while parsing.");
                return 7;
            }
        }

        /// <summary>
        /// Constructs a parser for the given input
        /// </summary>
        /// <param name="input">The input file or "-" for standard output</param>
        /// <param name="test">Whether or not to use the test string override</param>
        /// <param name="friendlyName">A friendly name to use for reporting errors</param>
        /// <returns>The created parser</returns>
        private static SemgusParser GetParser(string input, bool test, out string friendlyName)
        {
            if (test)
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(TestInput.TestInputString);
                writer.Flush();
                stream.Position = 0;
                friendlyName = "test input";
                return new SemgusParser(stream, "string");
            }
            else if (input == "-")
            {
                friendlyName = "standard input";
                return new SemgusParser(Console.In, "stdin");
            }
            else
            {
                friendlyName = input;
                return new SemgusParser(input);
            }
        }

        /// <summary>
        /// Gets an output writer for the given output string
        /// </summary>
        /// <param name="output">The output file, or "-" for standard output</param>
        /// <param name="writer">The output writer</param>
        /// <returns>A (possibly-null) IDisposable associated with the output writer</returns>
        private static IDisposable? GetOutputWriter(string output, out TextWriter writer)
        {
            if (output == "-")
            {
                writer = Console.Out;
            }
            else
            {
                writer = File.CreateText(output);
            }
            return writer as IDisposable;
        }

        /// <summary>
        /// Gets a problem handler for the given options
        /// </summary>
        /// <param name="writer">The output writer</param>
        /// <param name="mode">The selected processing mode</param>
        /// <param name="format">The selected output format</param>
        /// <param name="handler">The created handler</param>
        /// <returns>A (possibly-null) IDisposable associated with the problem handler</returns>
        private static IDisposable? GetHandler(TextWriter writer, ProcessingMode mode, OutputFormat format, out ISemgusProblemHandler handler)
        {
            switch (format)
            {
                case OutputFormat.Verify:
                    handler = new VerificationHandler(writer);
                    break;

                case OutputFormat.Json:
                    handler = new JsonHandler(writer, mode);
                    break;

                case OutputFormat.Sexpr:
                    handler = new SexprHandler(writer);
                    break;

                default:
                    // This should be caught by argument validation before this point
                    throw new ArgumentException("Not a valid format: " + format);
            }
            return handler as IDisposable;
        }
    }
}
