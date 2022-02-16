using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt;

namespace Semgus.Model
{
    public record SemgusSynthFun(SmtFunction Relation, SmtFunctionRank Rank, SemgusGrammar Grammar);
}
