using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt
{
    public interface ISmtTheory
    {
        SmtIdentifier Name { get; }
        IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts { get; }
        IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions { get; }
    }
}
