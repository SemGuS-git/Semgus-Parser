using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt.Terms;

namespace Semgus.Parser.Reader
{
    internal class ErrorTerm : SmtTerm
    {
        public string Message { get; }

        public ErrorTerm(string message) : base(ErrorSort.Instance)
        {
            Message = message;
            Console.WriteLine(message);
        }
    }
}
