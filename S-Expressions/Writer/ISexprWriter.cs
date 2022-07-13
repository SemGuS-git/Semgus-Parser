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
        public void WriteListStart();

        public void WriteListEnd();

        public void WriteList(Action contents);


        public void WriteNil();

        public void WriteSymbol(string name);

        public void WriteKeyword(string keyword);

        public void WriteString(string value);

        public void WriteNumeral(long value);

        public void WriteDecimal(double value);

        public void WriteBitVector(BitArray value);
    }
}
