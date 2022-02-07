using Semgus.Model;
using Semgus.Model.Smt;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Example
{
    internal class ExampleSemgusProblemHandler : ISemgusProblemHandler
    {
        public void AddLibraryFunction(SmtFunction fun)
        {
            throw new NotImplementedException();
        }

        public void AddSynthFun()
        {
            throw new NotImplementedException();
        }

        public void OnTermTypes(IReadOnlyList<TermType> termTypes)
        {
            Console.WriteLine("declare-term-types: ");
            foreach (var tt in termTypes)
            {
                Console.Write("  " + tt.Name.Symbol + " -->");
                bool firstConstructor = true;
                foreach (var cons in tt.Constructors)
                {
                    if (!firstConstructor)
                    {
                        Console.Write(" |");
                    }
                    else
                    {
                        firstConstructor = false;
                    }
                    Console.Write(" ");
                    Console.Write(cons.Operator.Symbol + "(");
                    bool firstChild = true;
                    foreach (var child in cons.Children)
                    {
                        if (!firstChild)
                        {
                            Console.Write(", ");
                        }
                        else
                        {
                            firstChild = false;
                        }
                        Console.Write(child.Name);
                    }
                    Console.Write(")");
                }
                Console.WriteLine(";");
            }
        }

        public void OnSetInfo(SmtContext ctx, SmtKeyword keyword)
        {
            Console.WriteLine("set-info: " + keyword.Name);
        }

        public void OnSynthFun(SmtContext ctx, SmtIdentifier name, IList<SmtConstant> args, SmtSort sort)
        {
            Console.WriteLine("synth-fun: " + name);
        }
    }
}
