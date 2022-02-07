using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt;

namespace Semgus.Model
{
    public class TermType : SmtSort
    {
        public TermType(SmtIdentifier termname) : base(termname) { }
        public IList<Constructor> Constructors { get; } = new List<Constructor>();
        public void AddConstructor(Constructor constructor)
        {
            Constructors.Add(constructor);
        }

        public record Constructor(SmtIdentifier Operator, params SmtSort[] Children);
    }
}
