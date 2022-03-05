using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Verifier
{
    /// <summary>
    /// A problem handler that dumps the output of a SemGuS problem directly to text for human verification
    /// </summary>
    internal class VerificationHandler : ISemgusProblemHandler
    {
        private readonly TextWriter _writer;

        public VerificationHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public void OnTermTypes(IReadOnlyList<SemgusTermType> termTypes)
        {
            _writer.WriteLine("declare-term-types: ");
            foreach (var tt in termTypes)
            {
                _writer.Write("  " + tt.Name.Symbol + " -->");
                bool firstConstructor = true;
                foreach (var cons in tt.Constructors)
                {
                    if (!firstConstructor)
                    {
                        _writer.Write(" |");
                    }
                    else
                    {
                        firstConstructor = false;
                    }
                    _writer.Write(" ");
                    _writer.Write(cons.Operator.Symbol + "(");
                    bool firstChild = true;
                    foreach (var child in cons.Children)
                    {
                        if (!firstChild)
                        {
                            _writer.Write(", ");
                        }
                        else
                        {
                            firstChild = false;
                        }
                        _writer.Write(child.Name);
                    }
                    _writer.Write(")");
                }
                _writer.WriteLine(";");
            }
        }

        public void OnSetInfo(SmtContext ctx, SmtAttribute attr)
        {
            _writer.WriteLine("set-info: " + attr.Keyword + " " + attr.Value);
        }

        public void OnSynthFun(SmtContext ctx, SmtIdentifier name, IList<SmtConstant> args, SmtSort sort)
        {
        }

        public void OnConstraint(SmtContext smtCtx, SemgusContext semgusCxt, SmtTerm constraint)
        {
        }

        public void OnCheckSynth(SmtContext smtCtx, SemgusContext semgusCtx)
        {
            _writer.WriteLine("check-synth");
            foreach (var chc in semgusCtx.Chcs)
            {
                _writer.WriteLine("CHC: " + chc.Head + " <= " + (chc.BodyRelations.Any() ? String.Join(" ^ ", chc.BodyRelations) + " ^ " : "") + chc.Constraint);
                _writer.Write($"    [constructor: ({string.Join(' ', chc.Binder.Bindings.Select(b => b.Binding.Id).Prepend(chc.Binder.Constructor!.Operator))})]");
                if (chc.InputVariables != null)
                {
                    _writer.Write($"     [inputs: {string.Join(' ', chc.InputVariables)}]");
                }
                if (chc.OutputVariables != null)
                {
                    _writer.Write($"     [outputs: {string.Join(' ', chc.OutputVariables)}]");
                }
                if (chc.InputVariables != null || chc.OutputVariables == null)
                {
                    _writer.WriteLine();
                }
            }

            foreach (var constraint in semgusCtx.Constraints)
            {
                _writer.WriteLine("Constraint: " + constraint);
            }

            foreach (var sf in semgusCtx.SynthFuns)
            {
                _writer.WriteLine("Function to Synthesize: " + sf.Relation.Name);
                _writer.WriteLine("Grammar: " + string.Join(' ', sf.Grammar.NonTerminals));
                foreach (var p in sf.Grammar.Productions)
                {
                    _writer.WriteLine(p);
                }
            }
        }
    }
}
