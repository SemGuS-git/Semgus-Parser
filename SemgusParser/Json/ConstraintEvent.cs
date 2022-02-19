using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Json
{
    internal class ConstraintEvent : ParseEvent
    {
        public SmtTerm Constraint { get; }

        public ConstraintEvent(SmtTerm constraint) : base("constraint", "semgus")
        {
            Constraint = constraint;
        }
    }
}
