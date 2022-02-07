using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Commands
{
    internal class TopLevelCommandAttribute : Attribute
    {
        public string Name { get; }

        public TopLevelCommandAttribute(string name)
        {
            Name = name;
        }
    }
}
