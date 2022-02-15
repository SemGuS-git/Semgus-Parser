using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model
{
    public class SemgusChc
    {
        public IReadOnlyList<SmtVariableBinding> VariableBindings { get; }

        public SemanticRelation Head { get; }

        public IReadOnlyList<SemanticRelation> BodyRelations { get; }

        public SmtTerm Constraint { get; }

        public SmtMatchBinder Binder { get; }

        public SemgusChc(SemanticRelation head, IEnumerable<SemanticRelation> childRels, SmtTerm constraint, SmtMatchBinder binder, IEnumerable<SmtVariableBinding> bindings)
        {
            VariableBindings = bindings.ToList();
            Head = head;
            BodyRelations = childRels.ToList();
            Binder = binder;
            Constraint = constraint;
        }

        public record SemanticRelation(SmtFunction Relation, SmtFunctionRank Rank, IReadOnlyList<SmtVariable> Arguments)
        {
            public override string ToString()
            {
                return $"({Relation.Name} {string.Join(' ', Arguments)})";
            }
        }
    }
}
