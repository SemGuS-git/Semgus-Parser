using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    public class SmtMatchBinder : SmtBinder
    {
        public IReadOnlyList<SmtMatchVariableBinding> Bindings { get; }
        public SemgusTermType.Constructor? Constructor { get; }
        public SemgusTermType ParentType { get; }
        public SmtMatchBinder(SmtTerm child, SmtScope newScope, SemgusTermType parentType, SemgusTermType.Constructor? constructor, IEnumerable<SmtMatchVariableBinding> bindings) : base(child, newScope)
        {
            Bindings = bindings.ToList();
            Constructor = constructor;
            ParentType = parentType;
        }

        public override TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor)
        {
            return visitor.VisitMatchBinder(this);
        }
    }

    public record SmtMatchVariableBinding(SmtVariableBinding Binding, int Index)
    {
        public const int FullTerm = -1;
    }

    public class SmtMatchGrouper : SmtTerm
    {
        public IReadOnlyList<SmtMatchBinder> Binders { get; }
        public SmtTerm Term { get; }
        public SmtMatchGrouper(SmtTerm term, SmtSort retSort, IEnumerable<SmtMatchBinder> patterns) : base(retSort)
        {
            Binders = patterns.ToList();
            Term = term;
        }

        public override TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor)
        {
            return visitor.VisitMatchGrouper(this);
        }
    }
}
