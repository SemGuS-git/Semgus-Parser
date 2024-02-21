using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Sexpr.Writer
{
    public class SexprWriter : ISexprWriter
    {
        private readonly TextWriter _tw;

        private bool _lastEndParen = false;
        private bool _lastStartParen = false;
        private int _listCount = 0;
        private bool _pretty = false;
        private int _maxLineLength;
        private bool _autoDelimit;

        private void MaybeWS(bool endParen = false, bool startParen = false)
        {
            if (!_lastStartParen && !(endParen))
            {
                if (_autoDelimit)
                {
                    WriteText(" ");
                }
            }
            if (_listCount == 0)
            {
                _tw.WriteLine();
            }
            _lastEndParen = endParen;
            _lastStartParen = startParen;
        }

        public SexprWriter(TextWriter tw, bool pretty = false, int maxLineLength = 120, bool autoDelimit = true)
        {
            _tw = tw;
            _pretty = pretty;
            _maxLineLength = maxLineLength;
            _autoDelimit = autoDelimit;
        }

        public void WriteNil()
        {
            MaybeWS();
            WriteText("nil");
        }
        public void WriteBitVector(BitArray value)
        {
            MaybeWS();

            WriteText("#*");
            // MSB first
            for (int i = value.Length - 1; i >= 0; --i)
            {
                WriteText(value[i] ? "1" : "0");
            }
        }

        public void WriteDecimal(double value)
        {
            MaybeWS();
            WriteText($"{value}");
        }

        public void WriteKeyword(string keyword)
        {
            MaybeWS();
            WriteText($":{keyword}"); // TODO: handle invalid characters
        }

        public void WriteList(Action contents)
        {
            MaybeWS(startParen: true);
            _listCount += 1;
            WithinLogicalBlock("(", ")", false, contents);
            _listCount -= 1;
            MaybeWS(endParen: true);
        }

        public void WriteNumeral(long value)
        {
            MaybeWS();
            WriteText($"{value}");
        }

        public void WriteString(string value)
        {
            MaybeWS();
            value = value.Replace("\"", "\\\"");
            WriteText($"\"{value}\"");
        }

        public void WriteSymbol(string name)
        {
            MaybeWS();
            WriteText(name); // TODO: check invalid characters
        }

        public void WriteText(string? text)
        {
            if (text is not null)
            {
                if (_currentSection is null)
                {
                    // Not pretty-printing
                    _tw.Write(text);
                }
                else
                {
                    _currentSection.Components.Add(new LiteralText(text));
                }
            }
        }

        public interface IPPComponent
        {
            public int Length { get; }
            public string Render();
        }
        public record LiteralText(string Text) : IPPComponent
        {
            public int Length => Text.Length;
            public string Render() => Text;
        }

        public record LogicalBlockStart(int BlockId, string Prefix = "", bool PerLinePrefix = false) : IPPComponent
        {
            public int Length => Prefix.Length;
            public string Render() => Prefix;
        }

        public record LogicalBlockEnd(int BlockId, string Suffix = "") : IPPComponent
        {
            public int Length => Suffix.Length;
            public string Render() => Suffix;
        }

        /// <summary>
        /// Sections have literal text, logical blocks, and indentation markers
        /// </summary>
        public class Section : IPPComponent
        {
            public Section? Enclosing { get; init; }
            public int Depth { get; init; }
            public List<IPPComponent> Components { get; set; } = new();
            public int Length => Components.Aggregate(seed: 0, func: (val, comp) => comp.Length + val);
            public string Render()
            {
                StringBuilder sb = new();
                foreach (var comp in Components)
                {
                    sb.Append(comp.Render());
                }
                return sb.ToString();
            }
        }

        public class ConditionalNewline : IPPComponent
        {
            public ISexprWriter.ConditionalNewlineKind Kind { get; init; }
            public int Length => 0;
            public string Render() => "";
        }

        public record Indentation(ISexprWriter.LogicalBlockRelativeTo RelativeTo, int N) : IPPComponent
        {
            public int Length => 0;
            public string Render() => "";
        }

        private Section? _currentSection = null;
        private int _logicalBlockDepth = 0;
        private int _logicalBlockIdCounter = 0;

        public void WithinLogicalBlock(string? prefix, string? suffix, bool perLinePrefix, Action body)
        {
            if (_currentSection is null)
            {
                _currentSection = new Section()
                {
                    Enclosing = null,
                    Depth = 0
                };
            }
            _logicalBlockDepth += 1;
            int id = ++_logicalBlockIdCounter;
            _currentSection.Components.Add(new LogicalBlockStart(id, prefix ?? "", perLinePrefix));
            Section newSection = new()
            {
                Enclosing = _currentSection,
                Depth = _logicalBlockDepth
            };
            _currentSection.Components.Add(newSection);
            _currentSection = newSection;
            body();
            // Note that the close of a logical block does not close the current section
            _currentSection.Components.Add(new LogicalBlockEnd(id, suffix ?? ""));
            _logicalBlockDepth -= 1;

            if (_logicalBlockDepth <= 0)
            {
                // Close open sections
                while (_currentSection.Enclosing is not null)
                {
                    _currentSection = _currentSection.Enclosing;
                }
                WriteOutPrettyText(_currentSection);
                _currentSection = null;
            }
        }

        public void AddConditionalNewline(ISexprWriter.ConditionalNewlineKind kind = ISexprWriter.ConditionalNewlineKind.Linear, bool skipAtSectionStart = true)
        {
            if (kind != ISexprWriter.ConditionalNewlineKind.Linear)
            {
                throw new NotImplementedException("Only linear newlines are currently implemented.");
            }

            if (skipAtSectionStart && _currentSection?.Components.Count == 0)
            {
                return; // Makes things a lot easier, so we don't have to check for section starts
            }

            // We have to close all sections at a nesting level deeper or equal to the current level
            while (_currentSection?.Depth >= _logicalBlockDepth)
            {
                _currentSection = _currentSection.Enclosing;
            }
            _currentSection?.Components.Add(new ConditionalNewline() { Kind = kind });
            Section newSection = new()
            {
                Enclosing = _currentSection,
                Depth = _logicalBlockDepth
            };
            _currentSection?.Components.Add(newSection);
            _currentSection = newSection;
        }

        public void LogicalBlockIndent(ISexprWriter.LogicalBlockRelativeTo relativeTo, int n)
        {
            if (_currentSection is null)
            {
                throw new InvalidOperationException("Cannot add an indent while not in a section.");
            }
            _currentSection.Components.Add(new Indentation(relativeTo, n));
        }

        private class LogicalBlockInfo
        {
            public int BlockId { get; init; }
            public int Indentation { get; set; }
            public string? LinePrefix { get; init; }
            public int PrefixIndentation { get; init; }
            public int BlockIndentation { get; init; }
        }

        private void WriteOutPrettyText(Section toplevel, int leftMargin = 0, Stack<LogicalBlockInfo>? logicalBlockStack = null)
        {
            if (logicalBlockStack is null)
            {
                logicalBlockStack = new();
            }

            bool breakSection = toplevel.Length + leftMargin > _maxLineLength;
                int currentPosition = leftMargin;

                bool AdvanceTo(int position)
                {
                    if (position < currentPosition)
                    {
                        return false;
                    }
                    else
                    {
                        _tw.Write(new string(' ', position - currentPosition));
                        currentPosition = position;
                        return true;
                    }
                }

            foreach (var comp in toplevel.Components)
            {
                if (comp is ConditionalNewline && breakSection)
                {
                    _tw.WriteLine();
                    currentPosition = 0;
                    foreach (var blockInfo in logicalBlockStack.Reverse())
                    {
                        if (blockInfo.LinePrefix is not null)
                        {
                            AdvanceTo(blockInfo.PrefixIndentation);
                            _tw.Write(blockInfo.LinePrefix);
                            currentPosition += blockInfo.LinePrefix.Length;
                        }
                    }

                    if (logicalBlockStack.TryPeek(out var topBlock))
                    {
                        AdvanceTo(topBlock.Indentation);
                    }
                }
                if (comp is Section section)
                {
                    WriteOutPrettyText(section, currentPosition, logicalBlockStack);
                }
                if (comp is LiteralText text)
                {
                    _tw.Write(text.Text);
                    currentPosition += text.Length;
                }
                if (comp is LogicalBlockStart lbs)
                {
                    _tw.Write(lbs.Prefix);
                    int prefixIndentation = currentPosition;
                    currentPosition += lbs.Prefix.Length;
                    logicalBlockStack.Push(new LogicalBlockInfo()
                    {
                        BlockId = lbs.BlockId,
                        LinePrefix = lbs.PerLinePrefix ? lbs.Prefix : null,
                        Indentation = currentPosition,
                        PrefixIndentation = prefixIndentation,
                        BlockIndentation = currentPosition
                    });
                }
                if (comp is LogicalBlockEnd lbe)
                {
                    _tw.Write(lbe.Suffix);
                    var blockInfo = logicalBlockStack.Pop();
                    if (blockInfo.BlockId != lbe.BlockId)
                    {
                        throw new InvalidOperationException($"Block ID mismatch. Got {lbe.BlockId}, but expected {blockInfo.BlockId}");
                    }
                }
                if (comp is Indentation ind)
                {
                    if (!logicalBlockStack.TryPeek(out var currentBlock))
                    {
                        throw new InvalidOperationException("Cannot indent while not inside a block.");
                    }

                    if (ind.RelativeTo == ISexprWriter.LogicalBlockRelativeTo.Block)
                    {
                        currentBlock.Indentation = currentBlock.BlockIndentation + ind.N;
                    }
                    else
                    {
                        currentBlock.Indentation = currentPosition + ind.N;
                    }
                }
            }
        }
    }
}
