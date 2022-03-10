using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Semgus.Model.Smt
{
    public interface ISmtTheory
    {
        IReadOnlyDictionary<SmtIdentifier, SmtSort> Sorts { get; }
        IReadOnlyDictionary<SmtIdentifier, SmtFunction> Functions { get; }
    }
}
