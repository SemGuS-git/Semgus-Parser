using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    public class SmtVariable : SmtTerm
    {
        public SmtVariable(SmtIdentifier name, SmtVariableBinding binding) : base(binding.Sort)
        {
            Name = name;
            Binding = binding;
        }

        public SmtIdentifier Name { get; }
        public SmtVariableBinding Binding { get; }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
