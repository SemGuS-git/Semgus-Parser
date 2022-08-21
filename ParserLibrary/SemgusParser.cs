using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;
using Semgus.Model.Smt.Transforms;
using Semgus.Parser.Commands;
using Semgus.Parser.Commands.Sygus;
using Semgus.Parser.Reader;
using Semgus.Sexpr.Reader;

namespace Semgus.Parser
{
    public class SemgusParser : IDisposable, ISourceContextProvider, IExtensionHandler
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
        /// The current SMT user-defined source
        /// </summary>
        public SmtUserDefinedSource CurrentSmtSource { get; }

        /// <summary>
        /// Creates a new SemGuS parser from the given file
        /// </summary>
        /// <param name="filename">Name of file to parse</param>
        public SemgusParser(string filename) : this(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), filename)
        {
            _filename = filename;
            CurrentSmtSource = SmtUserDefinedSource.ForFile(filename);
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
            CurrentSmtSource = SmtUserDefinedSource.ForStream(sourceName);
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
            CurrentSmtSource = SmtUserDefinedSource.ForStream(sourceName);
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
            procClass(typeof(DeclareSortCommand));

            // SyGuS extensions
            procClass(typeof(SetLogicCommand));
            procClass(typeof(DeclareVarCommand));

            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<ISmtConverter, Reader.Converters.SmtConverter>();
            services.AddSingleton<DestructuringHelper>();
            services.AddSingleton<ISuggestionGenerator, DidYouMean>();
            services.AddSingleton<ISourceMap, SourceMap>();
            services.AddScoped<ISmtContextProvider, SmtContextProvider>();
            services.AddScoped<ISmtScopeProvider, SmtScopeProvider>();
            services.AddScoped<ISemgusContextProvider, SemgusContextProvider>();
            services.AddSingleton<ISourceContextProvider>(this);
            services.AddSingleton<IExtensionHandler>(this);
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

                                var commandBody = ((IConsOrNil)cons).Rest();
                                if (commandBody is null)
                                {
                                    errorStream.WriteLine($"error: body for command {commandName} is not a proper list.");
                                    errCount += 1;
                                }
                                else if (!destructuringHelper.TryDestructureAndInvoke(command, commandBody, instance))
                                {
                                    errorStream.WriteLine("error: failed to find matching command signature for: " + commandName.Name);
                                    errCount += 1;
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

        /// <summary>
        /// Holds extensions that we have seen so far while parsing
        /// </summary>
        private ISet<SmtExtensionFinder.Extension> _reportedExtensions = new HashSet<SmtExtensionFinder.Extension>();
        
        /// <summary>
        /// Processes extensions and emits definitions for new extensions
        /// </summary>
        /// <param name="handler">The problem handler</param>
        /// <param name="ctx">The SMT context</param>
        /// <param name="term">The term to process</param>
        /// <exception cref="InvalidOperationException">Thrown if an extension is missing a definition</exception>
        public void ProcessExtensions(ISemgusProblemHandler handler, SmtContext ctx, SmtTerm term)
        {
            var extensions = SmtExtensionFinder.Find(term);
            extensions.ExceptWith(_reportedExtensions);

            foreach (var ext in extensions)
            {
                if (ext.Function.TryGetDefinition(ctx, ext.Rank, out var defn))
                {
                    handler.OnFunctionDeclaration(ctx, ext.Function, ext.Rank);
                    handler.OnFunctionDefinition(ctx, ext.Function, ext.Rank, defn);
                }
                else
                {
                    throw new InvalidOperationException($"Missing extension definition: {ext.Function.Name} [{ext.Rank}]");
                }
            }
            _reportedExtensions.UnionWith(extensions);
        }
    }
}
