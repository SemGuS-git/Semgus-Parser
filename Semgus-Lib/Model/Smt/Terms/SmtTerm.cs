using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Terms
{
    public abstract class SmtTerm
    {
        public SmtTerm(SmtSort sort)
        {
            Sort = sort;
        }

        public void AddAttribute(SmtAttribute attr)
        {
            if (_attributes == null)
            {
                _attributes = new();
            }
            _attributes.Add(attr);
        }

        private HashSet<SmtAttribute>? _attributes;
        public IReadOnlySet<SmtAttribute>? Annotations => _attributes;

        public SmtSort Sort { get; }

        public abstract TOutput Accept<TOutput>(ISmtTermVisitor<TOutput> visitor);
    }
}
