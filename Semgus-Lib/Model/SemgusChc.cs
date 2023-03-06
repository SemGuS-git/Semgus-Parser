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
        /// <summary>
        /// Top-level symbols used in this CHC
        /// </summary>
        public SymbolTable Symbols { get; }

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
            Symbols = new SymbolTable(this);
        }

        public record SemanticRelation(IApplicable Relation, SmtFunctionRank Rank, IReadOnlyList<SmtVariable> Arguments)
        {
            public override string ToString()
            {
                return $"({Relation.Name} {string.Join(' ', Arguments)})";
            }
        }

        /// <summary>
        /// Table of top-level symbols used by a CHC
        /// </summary>
        public class SymbolTable
        {
            public IReadOnlyCollection<SymbolEntry>? Inputs { get; }
            public IReadOnlyCollection<SymbolEntry>? Outputs { get; }
            public SymbolEntry Term { get; }
            public IReadOnlyCollection<SymbolEntry> Auxiliary { get; }
            public IReadOnlyCollection<SymbolEntry> Children { get; }
            private readonly SemgusChc _chc;

            /// <summary>
            /// Creates a new symbol table for the given CHC
            /// </summary>
            /// <param name="chc">CHC to create symbol table for</param>
            public SymbolTable(SemgusChc chc)
            {
                _chc = chc;
                if (chc.InputVariables is not null)
                {
                    Inputs = chc.InputVariables.Select(ConvertVariable).ToList();
                }
                if (chc.OutputVariables is not null)
                {
                    Outputs = chc.OutputVariables.Select(ConvertVariable).ToList();
                }
                Term = ConvertVariable(_chc.TermVariable);
                Auxiliary = chc.AuxiliaryVariables.Select(ConvertBinding).ToList();
                Children = chc.Binder.Bindings.Select(ConvertMatchBinding).ToList();
            }

            /// <summary>
            /// Converts a variable to a symbol entry
            /// </summary>
            /// <param name="v">Variable to write</param>
            /// <returns>Symbol entry for binding</returns>
            private SymbolEntry ConvertVariable(SmtVariable v)
            {
                return new SymbolEntry(v.Name, v.Sort.Name, FindIndex(v.Name));
            }

            /// <summary>
            /// Converts a variable binding to a symbol entry
            /// </summary>
            /// <param name="v">Binding to convert</param>
            /// <returns>Symbol entry for binding</returns>
            private SymbolEntry ConvertBinding(SmtVariableBinding v)
            {
                return new SymbolEntry(v.Id, v.Sort.Name);
            }

            /// <summary>
            /// Converts a match variable binding to a symbol entry
            /// </summary>
            /// <param name="v">Binding to convert</param>
            /// <returns>Symbol entry for binding</returns>
            private SymbolEntry ConvertMatchBinding(SmtMatchVariableBinding v)
            {
                return new SymbolEntry(v.Binding.Id, v.Binding.Sort.Name, v.Index);
            }

            /// <summary>
            /// Finds the index of an identifier in the CHC head's signature
            /// </summary>
            /// <param name="name">The identifier to find</param>
            /// <returns>The index</returns>
            /// <exception cref="InvalidOperationException">Thrown if the identifier isn't found</exception>
            private int FindIndex(SmtIdentifier name)
            {
                for (int i = 0; i < _chc.Head.Arguments.Count; ++i)
                {
                    if (_chc.Head.Arguments[i].Name == name)
                    {
                        return i;
                    }
                }
                throw new InvalidOperationException("Trying to find variable in CHC head that doesn't exist");
            }
        }

        /// <summary>
        /// A symbol entry for sorted symbols
        /// </summary>
        /// <param name="Id">Identifier of this symbol</param>
        /// <param name="Sort">Sort of this symbol</param>
        /// <param name="Index">Optional index of this symbol in the CHC head</param>
        public record SymbolEntry(SmtIdentifier Id, SmtSortIdentifier Sort, int? Index = default);
    }
}
