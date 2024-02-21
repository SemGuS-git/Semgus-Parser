using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Sexpr.Writer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Semgus
{
    /// <summary>
    /// Outputs a SemGuS problem to an S-expression stream
    /// </summary>
    internal class SemgusProblemOutputter
    {
        private ISexprWriter _sw;

        public void Output(SemgusContext sem, SmtContext smt)
        {

            foreach (var constraint in SemgusContext)
            OutputCheckSynth();
        }



        private void OutputCheckSynth()
        {
            _sw.WriteForm("check-synth");
        }
    }
}
