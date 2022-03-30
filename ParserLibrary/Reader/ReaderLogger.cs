using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Semgus.Sexpr.Reader;

#nullable enable

namespace Semgus.Parser.Reader
{
    /// <summary>
    /// An ILogger implementation for use in reading SemGuS files
    /// </summary>
    internal class ReaderLogger : ILogger
    {
        /// <summary>
        /// Writer that is logged to
        /// </summary>
        private readonly TextWriter _writer;

        /// <summary>
        /// Current stack of scopes, providing context information about where a message is logged
        /// </summary>
        private readonly Stack<string> _scopeStack;

        /// <summary>
        /// Provider for additional context information about errors
        /// </summary>
        private readonly ISourceContextProvider _sourceContextProvider;

        /// <summary>
        /// Constructs a new ReaderLogger with the given context provider and stream to log to
        /// </summary>
        /// <param name="scp">Source context provider</param>
        /// <param name="stream">Stream to log to</param>
        public ReaderLogger(ISourceContextProvider scp, Stream stream) : this(scp, new StreamWriter(stream))
        {
        }

        /// <summary>
        /// Constructs a new ReaderLogger with the given context provider and text writer to log to
        /// </summary>
        /// <param name="scp">Source context provider</param>
        /// <param name="tw">Text writer to log to</param>
        public ReaderLogger(ISourceContextProvider scp, TextWriter tw)
        {
            _sourceContextProvider = scp;
            _writer = tw;
            _scopeStack = new Stack<string>();
        }

        /// <summary>
        /// Helper class for automatically removing scopes when disposed
        /// </summary>
        private class ScopeStackPopper : IDisposable
        {
            /// <summary>
            /// The owning logger
            /// </summary>
            private readonly ReaderLogger _parent;

            /// <summary>
            /// Creates a new ScopeStackPopper for the given logger
            /// </summary>
            /// <param name="parent">The owning logger</param>
            public ScopeStackPopper(ReaderLogger parent)
            {
                _parent = parent;
            }

            /// <summary>
            /// Pops the most recent scope off of the parent logger
            /// </summary>
            public void Dispose()
            {
                _parent._scopeStack.Pop();
            }
        }

        /// <summary>
        /// Starts a new scope, providing context to future messages. The string representation of state is printed
        /// </summary>
        /// <typeparam name="TState">Arbitrary state type</typeparam>
        /// <param name="state">Scope state. Turned into a string and logged</param>
        /// <returns>Disposable that should be disposed of when the scope should be popped</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            _scopeStack.Push(state?.ToString() ?? "");
            return new ScopeStackPopper(this);
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var rls = state as ReaderLoggerState;
            if (rls is not null)
            {
                _writer.Write($"{rls.Position?.ToString() ?? "0:0:0"}: ");
            }
            _writer.Write(GetPrefix(logLevel));
            if (_scopeStack.Count > 0)
            {
                _writer.Write(string.Join(' ', _scopeStack.Reverse()) + " ");
            }
            _writer.Write(formatter(state, exception));
            _writer.WriteLine();

            if (rls is not null &&
                rls.Position is not null &&
                _sourceContextProvider.TryGetSourceLine(rls.Position, out var ctxLine))
            {
                _writer.WriteLine("Error reported here:");
                _writer.WriteLine(ctxLine);
                _writer.WriteLine(new string('-', rls.Position.Column - 1) + "^");
            }
        }

        /// <summary>
        /// Converts a log level into a prefix to log with
        /// </summary>
        /// <param name="ll">The log level</param>
        /// <returns>String prefix for logging</returns>
        private static string GetPrefix(LogLevel ll)
        {
            switch (ll)
            {
                case LogLevel.Trace: return "trace: ";
                case LogLevel.Debug: return "debug: ";
                case LogLevel.Information: return "info: ";
                case LogLevel.Warning: return "warning: ";
                case LogLevel.Error: return "error: ";
                case LogLevel.Critical: return "critical: ";
                default: return "msg: ";
            }
        }
    }

    /// <summary>
    /// State object for special logging with position and context information
    /// </summary>
    /// <param name="Message">The message to log</param>
    /// <param name="Position">The position of the reported error</param>
    internal record ReaderLoggerState(string Message, SexprPosition? Position);

    /// <summary>
    /// Extensions for common logging scenarios
    /// </summary>
    internal static class ReaderLoggerExtensions
    {
        /// <summary>
        /// Logs a parse error associated with the given position
        /// </summary>
        /// <typeparam name="T">Logger category type</typeparam>
        /// <param name="logger">This ILogger</param>
        /// <param name="msg">Message to log</param>
        /// <param name="pos">Position to report</param>
        public static void LogParseError<T>(this ILogger<T> logger, string msg, SexprPosition? pos)
        {
            logger.Log(LogLevel.Error, default, new ReaderLoggerState(msg, pos), null, (s, e) => s.Message);
        }

        /// <summary>
        /// Logs a parse error and throws a parse exception
        /// </summary>
        /// <typeparam name="T">Logger category type</typeparam>
        /// <param name="logger">This ILogger</param>
        /// <param name="msg">Message to log</param>
        /// <param name="pos">Position to report</param>
        /// <returns>An exception</returns>
        /// <exception cref="FatalParseException">Always thrown</exception>
        [DoesNotReturn]
        public static FatalParseException LogParseErrorAndThrow<T>(this ILogger<T> logger, string msg, SexprPosition? pos)
        {
            logger.LogParseError(msg, pos);
            throw new FatalParseException(msg, pos);
        }
    }

    /// <summary>
    /// Provider for the ReaderLogger
    /// </summary>
    internal class ReaderLoggerProvider : ILoggerProvider
    {
        /// <summary>
        /// Singleton logger instance to use
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new ReaderLoggerProvider
        /// </summary>
        /// <param name="scp">ISourceContextProvider for mapping positions to source contexts</param>
        /// <param name="tw">TextWriter to write log messages to</param>
        public ReaderLoggerProvider(ISourceContextProvider scp, TextWriter tw)
        {
            _logger = new ReaderLogger(scp, tw);
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
