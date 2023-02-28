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

        /// <summary>
        /// Variables in the CHC head marked as input variables.
        /// Null when this CHC is not annotated with input/output information
        /// </summary>
        public IReadOnlyCollection<SmtVariable>? InputVariables { get; }

        /// <summary>
        /// Variables in the CHC head marked as output variables.
        /// Null when this CHC is not annotated with input/output information
        /// </summary>
        public IReadOnlyCollection<SmtVariable>? OutputVariables { get; }

        /// <summary>
        /// Variables bound inside the CHC body (not appearing in the head)
        /// These come from a top-level exists clause in the SMT-LIB2 encoding
        /// </summary>
        public IReadOnlyCollection<SmtVariableBinding> AuxiliaryVariables { get; }

        /// <summary>
        /// The variable capturing the term this CHC is over
        /// </summary>
        public SmtVariable TermVariable { get; }

        public IReadOnlyList<SemanticRelation> BodyRelations { get; }

        public SmtTerm Constraint { get; }

        public SmtMatchBinder Binder { get; }

        public SemgusChc(SemanticRelation head, IEnumerable<SemanticRelation> childRels, SmtTerm constraint, SmtMatchBinder binder, IEnumerable<SmtVariableBinding> bindings,
                         SmtVariable term, IEnumerable<SmtVariableBinding> auxiliaries,
                         IEnumerable<SmtVariable>? inputs = default, IEnumerable<SmtVariable>? outputs = default)
        {
            VariableBindings = bindings.ToList();
            Head = head;
            BodyRelations = childRels.ToList();
            Binder = binder;
            Constraint = constraint;
            InputVariables = inputs?.ToList();
            OutputVariables = outputs?.ToList();
            AuxiliaryVariables = auxiliaries.ToList();
            TermVariable = term;
        }

        public record SemanticRelation(IApplicable Relation, SmtFunctionRank Rank, IReadOnlyList<SmtVariable> Arguments)
        {
            public override string ToString()
            {
                return $"({Relation.Name} {string.Join(' ', Arguments)})";
            }
        }
    }
}
