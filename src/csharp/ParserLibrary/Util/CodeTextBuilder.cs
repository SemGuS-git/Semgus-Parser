using System;
using System.Collections.Generic;
using System.Text;

namespace Semgus.Util {
    /// <summary>
    /// Wraps a StringBuilder with helper methods for indentation, delimiters, etc.
    /// </summary>
    public class CodeTextBuilder {
        private int _indentLevel = 0;
        private bool _pendingLineBreak = false;
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly int _indentSize;

        public CodeTextBuilder(int indentSize = 2) {
            this._indentSize = 2;
        }

        private string GetIndent(int n) => new string(' ', _indentSize * n);

        public IDisposable InDelimiters(string start, string end) {
            Write(start);
            Indent(1);

            return new ActionDisposable(() => {
                Outdent(1);
                Write(end);
            });
        }

        public IDisposable InParens() => InDelimiters("(", ")");
        public IDisposable InBrackets() => InDelimiters("[", "]");
        public IDisposable InBraces() => InDelimiters("{", "}");
        public IDisposable InLineBreaks() {
            LineBreak();
            return new ActionDisposable(() => LineBreak());
        }

        public CodeTextBuilder LineBreak() {
            _pendingLineBreak = true;
            return this;
        }

        public CodeTextBuilder Write(string text) {
            if (_pendingLineBreak) {
                _stringBuilder.AppendLine();
                _stringBuilder.Append(GetIndent(_indentLevel));
                _pendingLineBreak = false;
            }
            _stringBuilder.Append(text);
            return this;
        }

        public CodeTextBuilder WriteEach(IEnumerable<string> enumerable, string sep = " ") {
            bool first = true;
            foreach (var str in enumerable) {
                if (first) {
                    first = false;
                } else {
                    Write(sep);
                }
                Write(str);
            }
            return this;
        }

        public void Indent(int levels = 1) => _indentLevel += levels;
        public void Outdent(int levels = 1) => _indentLevel -= levels;

        public IDisposable IntentBlock(int levels = 1) {
            Indent(levels);
            return new ActionDisposable(() => Outdent(levels));
        }

        public override string ToString() => _stringBuilder.ToString();
    }
}
