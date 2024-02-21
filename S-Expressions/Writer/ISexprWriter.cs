using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Sexpr.Writer
{
    public interface ISexprWriter
    {
        public void WriteList(Action contents);


        public void WriteNil();

        public void WriteSymbol(string name);

        public void WriteKeyword(string keyword);

        public void WriteString(string value);

        public void WriteNumeral(long value);

        public void WriteDecimal(double value);

        public void WriteBitVector(BitArray value);

        public void WithinLogicalBlock(string prefix,
                                          string suffix,
                                          bool perLinePrefix,
                                          Action body);

        public enum ConditionalNewlineKind
        {
            Linear,
            Fill,
            Miser,
            Mandatory
        }

        public void AddConditionalNewline(ConditionalNewlineKind kind = ConditionalNewlineKind.Linear, bool skipAtBlockStart = true);

        public enum LogicalBlockRelativeTo
        {
            Block,
            Current
        }

        public void LogicalBlockIndent(LogicalBlockRelativeTo relativeTo, int n);
    }

    public static class PrettyPrinterExtensions
    {
        public static void LogicalBlockIndent(this ISexprWriter me, int n = 0)
            => me.LogicalBlockIndent(ISexprWriter.LogicalBlockRelativeTo.Block, n);

        public static void LogicalBlockCurrentIndent(this ISexprWriter me, int n = 0)
            => me.LogicalBlockIndent(ISexprWriter.LogicalBlockRelativeTo.Current, n);

    }
}
