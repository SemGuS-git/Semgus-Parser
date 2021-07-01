using Semgus.Parser.Commands;
using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;
using Semgus.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser
{
    public class SemgusParser : IDisposable
    {
        private readonly SemgusReader _reader;
        private readonly Stream _stream;
        private readonly IDictionary<string, ISemgusCommand> _commandDispatch;

        public SemgusParser(string filename)
        {
            _stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            _reader = new SemgusReader(_stream);
            _commandDispatch = new Dictionary<string, ISemgusCommand>();
            _commandDispatch.Add(new SynthFunCommand().AsKeyValuePair());
            _commandDispatch.Add(new ConstraintCommand().AsKeyValuePair());
            _commandDispatch.Add(new DeclareVarCommand().AsKeyValuePair());
        }

        public bool TryParse(out SemgusProblem problem, TextWriter errorStream = default)
        {
            if (default == errorStream)
            {
                errorStream = Console.Error;
            }

            LanguageEnvironment langEnv = new();

            problem = new(default, default, langEnv, new List<Constraint>());
            SemgusToken sexpr;
            int errCount = 0;
            while (_reader.EndOfFileSentinel != (sexpr = _reader.Read(errorOnEndOfStream: false)))
            {
                if (sexpr is not ConsToken cons)
                {
                    // Error: top-level symbols and literals not allowed
                    throw new InvalidOperationException("Top-level atoms not allowed: found " + sexpr.ToString() + " at " + sexpr.Position);
                }
                else
                {
                    //
                    // The first element should be a symbol telling us what it is
                    //
                    var head = cons.Head;
                    if (head is not SymbolToken commandName)
                    {
                        throw new InvalidOperationException("Expected command name, but got: " + sexpr.ToString() + " at " + sexpr.Position);
                    }
                    else if (!_commandDispatch.TryGetValue(commandName.Name, out var command))
                    {
                        throw new InvalidOperationException("Unknown top-level command: " + commandName.Name + " at " + commandName.ToString());
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

        public void Dispose()
        {
            ((IDisposable)_stream).Dispose();
        }
    }
}
