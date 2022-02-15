using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model
{
    public class SemgusContext
    {
        private readonly List<SmtTerm> _constraints = new();
        private readonly List<SemgusChc> _chcs = new();

        public void AddConstraint(SmtTerm constraint)
        {
            _constraints.Add(constraint);
        }

        public void AddChc(SemgusChc chc)
        {
            _chcs.Add(chc);
        }

        public IReadOnlyCollection<SemgusChc> Chcs => _chcs;
        public IReadOnlyCollection<SmtTerm> Constraints => _constraints;
    }
}
