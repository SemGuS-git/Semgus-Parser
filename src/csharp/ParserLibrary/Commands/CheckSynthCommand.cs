using Semgus.Parser.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Commands
{
    internal class CheckSynthCommand
    {
        [Command("check-synth")]
        public static void CheckSynth()
        {
            Console.WriteLine("Check Synth!!!");
        }
    }
}
