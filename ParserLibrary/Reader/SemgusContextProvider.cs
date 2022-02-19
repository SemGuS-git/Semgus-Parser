using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model;

namespace Semgus.Parser.Reader
{
    public interface ISemgusContextProvider
    {
        public SemgusContext Context { get; set; }
    }

    internal class SemgusContextProvider : ISemgusContextProvider
    {
        public SemgusContext Context { get; set; }
    }
}
