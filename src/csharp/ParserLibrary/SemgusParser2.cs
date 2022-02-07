using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Semgus.Model.Smt;
using Semgus.Parser.Commands;
using Semgus.Parser.Reader;
using Semgus.Syntax;

namespace Semgus.Parser
{
    public class SemgusParser2 : IDisposable
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
        private readonly IDictionary<string, MethodInfo> _commandDispatch;

        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new SemGuS parser from the given file
        /// </summary>
        /// <param name="filename">Name of file to parser</param>
        public SemgusParser2(string filename)
        {
            _stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            _reader = new SemgusReader(_stream);
            _reader.SetSourceName(filename);
            _commandDispatch = new Dictionary<string, MethodInfo>();
            _serviceProvider = ProcessCommandInfo();
        }

        public SemgusParser2(Stream stream, string sourceName)
        {
            _stream = stream;
            _reader = new SemgusReader(_stream);
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
            procClass(typeof(DefineFunsRecCommand));
            procClass(typeof(ConstraintCommand));

            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<SmtConverter>();
            services.AddSingleton<DestructuringHelper>();
            services.AddScoped<ISmtContextProvider, SmtContextProvider>();
            services.AddScoped<ISmtScopeProvider, SmtScopeProvider>();

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Attempts to parse a SemgusProblem from the underlying file
        /// </summary>
        /// <param name="problem">The parsed problem</param>
        /// <param name="errorStream">TextWriter for errors. Defaults to Console.Error</param>
        /// <returns>True if successfully parsed, false if one or more errors encountered</returns>
        public bool TryParse(ISemgusProblemHandler handler, TextWriter errorStream = default)
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
        public bool TryParse(ISemgusProblemHandler handler, out int errCount, TextWriter errorStream = default)
        {
            if (default == errorStream)
            {
                errorStream = Console.Error;
            }

            using (_serviceProvider.CreateScope())
            {
                var destructuringHelper = _serviceProvider.GetRequiredService<DestructuringHelper>();
                _serviceProvider.GetRequiredService<ISmtContextProvider>().Context = new SmtContext();
                using var scope = _serviceProvider.GetRequiredService<ISmtScopeProvider>().CreateNewScope();

                LanguageEnvironment langEnv = new();

                VariableClosure startingClosure = new(default, Array.Empty<VariableDeclaration>());

                //problem = new(default, startingClosure, langEnv, new List<Constraint>());
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
                            catch (InvalidOperationException ioe)
                            {
                                errorStream.WriteLine("Fatal error during parsing: " + ioe.Message);
                                errorStream.WriteLine("Full stack trace: \n" + ioe.ToString());
                                errCount += 1;
                                //problem = default;
                            }/*
                        if (problem is null)
                        {
                            errorStream.WriteLine("Terminating due to fatal error encountered while parsing command: " + commandName.Name);
                            return false;
                        }*/
                        }
                    }
                }
                return true;
            }
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
