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

        private void MaybeWS(bool endParen = false, bool startParen = false)
        {
            if (!_lastStartParen && !(endParen))
            {
                _tw.Write(" ");
            }
            if (_listCount == 0)
            {
                _tw.WriteLine();
            }
            _lastEndParen = endParen;
            _lastStartParen = startParen;
        }

        public SexprWriter(TextWriter tw)
        {
            _tw = tw;
        }

        public void WriteNil()
        {
            MaybeWS();
            _tw.Write("nil");
        }
        public void WriteBitVector(BitArray value)
        {
            MaybeWS();

            _tw.Write("#*");
            // MSB first
            for (int i = value.Length - 1; i >= 0; --i)
            {
                _tw.Write(value[i] ? "1" : "0");
            }
        }

        public void WriteDecimal(double value)
        {
            MaybeWS();

            _tw.Write(value);
        }

        public void WriteKeyword(string keyword)
        {
            MaybeWS();

            _tw.Write(":");
            _tw.Write(keyword); // TODO: handle invalid characters
        }

        public void WriteList(Action contents)
        {
            WriteListStart();
            contents();
            WriteListEnd();
        }

        public void WriteListEnd()
        {
            MaybeWS(endParen: true);
            _tw.Write(")");
            _listCount -= 1;
        }

        public void WriteListStart()
        {
            MaybeWS(startParen: true);
            _tw.Write("(");
            _listCount += 1;
        }

        public void WriteNumeral(long value)
        {
            MaybeWS();
            _tw.Write(value);
        }

        public void WriteString(string value)
        {
            MaybeWS();
            value = value.Replace("\"", "\\\"");
            _tw.Write($"\"{value}\"");
        }

        public void WriteSymbol(string name)
        {
            MaybeWS();
            _tw.Write(name); // TODO: check invalid characters
        }
    }
}
