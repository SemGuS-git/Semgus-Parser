using System;
using System.Collections.Generic;
using System.IO;

using Semgus.Parser.Commands;
using Semgus.Parser.Reader;
using Semgus.Syntax;

namespace Semgus.Parser
{
    /// <summary>
    /// Parser that reads SemGuS files in S-expression format and turns them into SemgusProblems
    /// </summary>
    public class SemgusParser : IDisposable
    {
        /// <summary>
        /// The underlying S-expression reader
        /// </summary>
        private readonly SemgusReader _reader;

        /// <summary>
        /// Backing stream that the reader reads from
        /// </summary>
        private readonly Stream _stream;

        /// <summary>
        /// Mapping of command names to command objects
        /// </summary>
        private readonly IDictionary<string, ISemgusCommand> _commandDispatch;

        /// <summary>
        /// Creates a new SemGuS parser from the given file
        /// </summary>
        /// <param name="filename">Name of file to parser</param>
        public SemgusParser(string filename)
        {
            _stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            _reader = new SemgusReader(_stream);
            _commandDispatch = new Dictionary<string, ISemgusCommand>();
            _commandDispatch.Add(new SynthTermCommand().AsKeyValuePair());
            _commandDispatch.Add(new ConstraintCommand().AsKeyValuePair());
            _commandDispatch.Add(new DeclareVarCommand().AsKeyValuePair());
            _commandDispatch.Add(new MetadataCommand().AsKeyValuePair());
            _commandDispatch.Add(new DeclareTermTypeCommand().AsKeyValuePair());
        }

        /// <summary>
        /// Attempts to parse a SemgusProblem from the underlying file
        /// </summary>
        /// <param name="problem">The parsed problem</param>
        /// <param name="errorStream">TextWriter for errors. Defaults to Console.Error</param>
        /// <returns>True if successfully parsed, false if one or more errors encountered</returns>
        public bool TryParse(out SemgusProblem problem, TextWriter errorStream = default)
        {
            return TryParse(out problem, out int _, errorStream);
        }

        /// <summary>
        /// Attempts to parse a SemgusProblem from the underlying file
        /// </summary>
        /// <param name="problem">The parsed problem</param>
        /// <param name="errCount">Count of encountered errors</param>
        /// <param name="errorStream">TextWriter for errors. Defaults to Console.Error</param>
        /// <returns>True if successfully parsed, false if one or more errors encountered</returns>
        public bool TryParse(out SemgusProblem problem, out int errCount, TextWriter errorStream = default)
        {
            if (default == errorStream)
            {
                errorStream = Console.Error;
            }

            LanguageEnvironment langEnv = new();

            // This is a hack to make the name analysis phase not complain about true and false,
            // since they're not technically literals in SMT-LIB2 format, just symbols. It's the
            // Core theory that imbues these with meaning, and we need a better way of handling that.
            var boolType = langEnv.IncludeType("Bool");
            VariableClosure startingClosure = new(default, new[] {
                new VariableDeclaration("true",  boolType, VariableDeclaration.Context.CT_Auxiliary),
                new VariableDeclaration("false", boolType, VariableDeclaration.Context.CT_Auxiliary)
            });

            problem = new(default, startingClosure, langEnv, new List<Constraint>());
            SemgusToken sexpr;
            errCount = 0;
            while (_reader.EndOfFileSentinel != (sexpr = _reader.Read(errorOnEndOfStream: false)))
            {
                if (sexpr is not ConsToken cons)
                {
                    // Error: top-level symbols and literals not allowed
                    errorStream.WriteParseError("Top-level atoms not allowed: found " + sexpr.ToString(), sexpr.Position);
                    errCount += 1;
                    continue;
                }
                else
                {
                    //
                    // The first element should be a symbol telling us what it is
                    //
                    var head = cons.Head;
                    if (head is not SymbolToken commandName)
                    {
                        errorStream.WriteParseError("Expected command name, but got: " + sexpr.ToString(), sexpr.Position);
                        errCount += 1;
                        continue;
                    }
                    else if (!_commandDispatch.TryGetValue(commandName.Name, out var command))
                    {
                        errorStream.WriteParseError("Unknown top-level command: " + commandName.Name, commandName.Position);
                        errCount += 1;
                        continue;
                    }
                    else
                    {
                        problem = command.Process(problem, cons, errorStream, ref errCount);
                        if (problem is null)
                        {
                            errorStream.WriteLine("Terminating due to fatal error encountered while parsing command: " + commandName.Name);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Disposes the underlying stream
        /// </summary>
        public void Dispose()
        {
            ((IDisposable)_stream).Dispose();
        }
    }
}
