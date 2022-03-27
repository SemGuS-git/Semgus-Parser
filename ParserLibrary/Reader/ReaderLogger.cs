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
    internal class ReaderLogger : ILogger
    {
        private readonly TextWriter _writer;
        private readonly Stack<string> _scopeStack;

        public ReaderLogger(Stream stream) : this(new StreamWriter(stream))
        {
        }

        public ReaderLogger(TextWriter tw)
        {
            _writer = tw;
            _scopeStack = new Stack<string>();
        }

        private class ScopeStackPopper : IDisposable
        {
            private readonly ReaderLogger _parent;
            public ScopeStackPopper(ReaderLogger parent)
            {
                _parent = parent;
            }

            public void Dispose()
            {
                _parent._scopeStack.Pop();
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            _scopeStack.Push(state?.ToString() ?? "");
            return new ScopeStackPopper(this);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (state is ReaderLoggerState rls)
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
        }

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

    internal record ReaderLoggerState(string Message, SexprPosition? Position);

    public static class ReaderLoggerExtensions
    {
        public static void LogParseError<T>(this ILogger<T> logger, string msg, SexprPosition? pos)
        {
            logger.Log(LogLevel.Error, default, new ReaderLoggerState(msg, pos), null, (s, e) => s.Message);
        }

        [DoesNotReturn]
        public static void LogParseErrorAndThrow<T>(this ILogger<T> logger, string msg, SexprPosition? pos)
        {
            logger.LogParseError(msg, pos);
            throw new FatalParseException(msg, pos);
        }
    }

    internal class ReaderLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public ReaderLoggerProvider(TextWriter tw)
        {
            _logger = new ReaderLogger(tw);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose()
        {
        }
    }
}
