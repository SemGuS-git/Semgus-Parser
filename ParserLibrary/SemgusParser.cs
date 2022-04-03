using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Parser.Commands;
using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;

#nullable enable

namespace Semgus.Parser
{
    public class SemgusParser : IDisposable, ISourceContextProvider
    {
        /// <summary>
        /// The underlying S-expression reader
        /// </summary>
        private readonly SemgusReader _reader;

        /// <summary>
        /// Name of file we're reading from, if reading from a file
        /// </summary>
        private readonly string? _filename;

        /// <summary>
        /// Thing that we need to dispose at the end
        /// </summary>
        private readonly IDisposable _streamOrReader;

        /// <summary>
        /// Mapping of command names to command objects
        /// </summary>
        private readonly IDictionary<string, MethodInfo> _commandDispatch;

        /// <summary>
        /// Service provider for DI
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new SemGuS parser from the given file
        /// </summary>
        /// <param name="filename">Name of file to parse</param>
        public SemgusParser(string filename) : this(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), filename)
        {
            _filename = filename;
        }

        /// <summary>
        /// Creates a new SemGuS parser from the given stream
        /// </summary>
        /// <param name="stream">Stream to parse</param>
        /// <param name="sourceName">Informational name for reporting errors</param>
        public SemgusParser(Stream stream, string sourceName)
        {
            _streamOrReader = stream;
            _reader = new SemgusReader(stream);
            _reader.SetSourceName(sourceName);
            _commandDispatch = new Dictionary<string, MethodInfo>();
            _serviceProvider = ProcessCommandInfo();
        }

        /// <summary>
        /// Creates a new SemGuS parser from the given text reader
        /// </summary>
        /// <param name="reader">Text reader to parse</param>
        /// <param name="sourceName">Informational name for reporting errors</param>
        public SemgusParser(TextReader reader, string sourceName)
        {
            _streamOrReader = reader;
            _reader = new SemgusReader(reader);
            _reader.SetSourceName(sourceName);
            _commandDispatch = new Dictionary<string, MethodInfo>();
            _serviceProvider = ProcessCommandInfo();
        }

        private IServiceProvider ProcessCommandInfo()
        {
            void procClass(Type t)
            {
                foreach (var m in t.GetMethods())
                {
                    var cmdAttr = m.GetCustomAttribute<CommandAttribute>();
                    if (cmdAttr is not null)
                    {
                        _commandDispatch.Add(cmdAttr.Name, m);
                    }
                }
            }

            procClass(typeof(SetInfoCommand));
            procClass(typeof(SynthFunCommand));
            procClass(typeof(DeclareTermTypeCommand));
            procClass(typeof(FunctionDefinitionCommands));
            procClass(typeof(ConstraintCommand));
            procClass(typeof(CheckSynthCommand));

            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<ISmtConverter, Reader.Converters.SmtConverter>();
            services.AddSingleton<DestructuringHelper>();
            services.AddSingleton<ISuggestionGenerator, DidYouMean>();
            services.AddSingleton<ISourceMap, SourceMap>();
            services.AddScoped<ISmtContextProvider, SmtContextProvider>();
            services.AddScoped<ISmtScopeProvider, SmtScopeProvider>();
            services.AddScoped<ISemgusContextProvider, SemgusContextProvider>();
            services.AddLogging(config =>
            {
                config.AddProvider(new ReaderLoggerProvider(this, Console.Error));
            });

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Attempts to parse a SemgusProblem from the underlying file
        /// </summary>
        /// <param name="problem">The parsed problem</param>
        /// <param name="errorStream">TextWriter for errors. Defaults to Console.Error</param>
        /// <returns>True if successfully parsed, false if one or more errors encountered</returns>
        public bool TryParse(ISemgusProblemHandler handler, TextWriter? errorStream = default)
        {
            return TryParse(handler, out int _, errorStream);
        }

        /// <summary>
        /// Attempts to parse a SemgusProblem from the underlying file
        /// </summary>
        /// <param name="problem">The parsed problem</param>
        /// <param name="errCount">Count of encountered errors</param>
        /// <param name="errorStream">TextWriter for errors. Defaults to Console.Error</param>
        /// <returns>True if successfully parsed, false if one or more errors encountered</returns>
        public bool TryParse(ISemgusProblemHandler handler, out int errCount, TextWriter? errorStream = default)
        {
            if (default == errorStream)
            {
                errorStream = Console.Error;
            }

            using (_serviceProvider.CreateScope())
            {
                var destructuringHelper = _serviceProvider.GetRequiredService<DestructuringHelper>();
                _serviceProvider.GetRequiredService<ISmtContextProvider>().Context = new SmtContext();
                _serviceProvider.GetRequiredService<ISemgusContextProvider>().Context = new SemgusContext();
                using var scope = _serviceProvider.GetRequiredService<ISmtScopeProvider>().CreateNewScope();

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
                            try
                            {
                                object? instance = null;
                                if (!command.IsStatic)
                                {
                                    instance = ActivatorUtilities.CreateInstance(_serviceProvider, command.DeclaringType!, handler);
                                    if (instance is null)
                                    {
                                        throw new InvalidOperationException("Cannot instantiate command class: " + command.DeclaringType!.Name);
                                    }
                                }

                                if (!destructuringHelper.TryDestructureAndInvoke(command, ((IConsOrNil)cons).Rest(), instance))
                                {
                                    errorStream.WriteLine("Failed to find matching command signature for: " + commandName.Name);
                                }
                            }
                            catch (FatalParseException)
                            {
                                // Details should be logged when thrown
                                errorStream.WriteLine($"\nTerminated parsing `{commandName.Name}` command due to fatal error.");
                                errorStream.WriteLine($"{new string('=', 80)}\n");
                                errCount += 1;
                            }
                            catch (InvalidOperationException ioe)
                            {
                                errorStream.WriteLine("Fatal error during parsing: " + ioe.Message);
                                errorStream.WriteLine("Full stack trace: \n" + ioe.ToString());
                                errCount += 1;
                            }
                        }
                    }
                }
                return errCount == 0;
            }
        }

        /// <summary>
        /// Disposes the underlying stream
        /// </summary>
        public void Dispose()
        {
            ((IDisposable)_streamOrReader).Dispose();
        }

        /// <summary>
        /// Tries to get the full line of where an error occurred
        /// </summary>
        /// <param name="position">Position to look up</param>
        /// <returns>Source line</returns>
        public bool TryGetSourceLine(SexprPosition? position, out string? line)
        {
            if (position is null)
            {
                line = default;
                return false;
            }

            // Make sure this is an approved source
            // We don't want to be trying to read arbitrary files that have the
            // same name as other sources (e.g., stdin)
            if (position.Source == _filename)
            {
                line = File.ReadLines(position.Source).Skip(position.Line - 1).FirstOrDefault();
                return line is not null;
            }

            line = default;
            return false;
        }
    }
}
