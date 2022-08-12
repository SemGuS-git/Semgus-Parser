using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    public class SmtLambdaBinder : SmtBinder
    {
        public IReadOnlyList<SmtIdentifier> ArgumentNames { get; }

        public SmtLambdaBinder(SmtTerm child, SmtScope newScope, IEnumerable<SmtIdentifier> argNames) : base(child, newScope)
        {
            ArgumentNames = argNames.ToList();
        }

        public override TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor)
        {
            return visitor.VisitLambdaBinder(this);
        }

        public override string ToString()
        {
            return $"(lambda ({string.Join(' ', ArgumentNames)}) {Child})";
        }
    }
}
