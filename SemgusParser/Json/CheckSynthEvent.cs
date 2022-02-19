using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Json
{
    internal class CheckSynthEvent : ParseEvent
    {
        public CheckSynthEvent() : base("check-synth", "semgus") { }
    }
}
