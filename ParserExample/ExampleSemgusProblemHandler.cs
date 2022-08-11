using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Example
{
    internal class ExampleSemgusProblemHandler : ISemgusProblemHandler
    {
        public void OnTermTypes(IReadOnlyList<SemgusTermType> termTypes)
        {
            Console.WriteLine("declare-term-types: ");
            foreach (var tt in termTypes)
            {
                Console.Write("  " + tt.Name + " -->");
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

        public void OnSetInfo(SmtContext ctx, SmtAttribute attr)
        {
            Console.WriteLine("set-info: " + attr.Keyword + " : " + attr.Value);
        }

        public void OnSynthFun(SmtContext ctx, SmtIdentifier name, IList<(SmtIdentifier, SmtSortIdentifier)> args, SmtSort sort)
        {
            Console.WriteLine("synth-fun: " + name);
        }

        public void OnConstraint(SmtContext smtCtx, SemgusContext semgusCxt, SmtTerm constraint)
        {
            Console.WriteLine("constraint: " + constraint);
        }

        public void OnCheckSynth(SmtContext smtCtx, SemgusContext semgusCtx)
        {
            Console.WriteLine("check-synth");
            foreach (var chc in semgusCtx.Chcs)
            {
                Console.WriteLine("CHC: " + chc.Head + " <= " + (chc.BodyRelations.Any() ? String.Join(" ^ ", chc.BodyRelations) + " ^ " : "") + chc.Constraint);
                if (chc.InputVariables != null)
                {
                    Console.Write($"    (inputs: {string.Join(' ', chc.InputVariables)})");
                }
                if (chc.OutputVariables != null)
                {
                    Console.Write($"    (outputs: {string.Join(' ', chc.OutputVariables)})");
                }
                if (chc.InputVariables != null || chc.OutputVariables == null)
                {
                    Console.WriteLine();
                }
            }

            foreach (var constraint in semgusCtx.Constraints)
            {
                Console.WriteLine("Constraint: " + constraint);
            }

            foreach (var sf in semgusCtx.SynthFuns)
            {
                Console.WriteLine("Function to Synthesize: " + sf.Relation);
                Console.WriteLine("Grammar: " + string.Join(' ', sf.Grammar.NonTerminals));
                foreach (var p in sf.Grammar.Productions)
                {
                    Console.WriteLine(p);
                }
            }
        }
    }
}
