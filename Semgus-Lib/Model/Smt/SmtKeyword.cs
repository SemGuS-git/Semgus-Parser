using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    public record SmtKeyword(string Name)
    {
        public override string ToString()
        {
            return $":{Name}";
        }
    }
}
