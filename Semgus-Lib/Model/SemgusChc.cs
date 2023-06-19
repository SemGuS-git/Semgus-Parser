using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

namespace Semgus.Model
{
    /// <summary>
    /// A CHC.
    /// </summary>
    public class SemgusChc
    {
        /// <summary>
        /// Top-level symbols used in this CHC
        /// </summary>
        public SymbolTable Symbols { get; }

        /// A unique identifier for this CHC
        /// </summary>
        public SmtIdentifier? Id { get; init; }

        /// <summary>
        /// Auxiliary variables bound in this CHC
        /// </summary>
        public IReadOnlyList<SmtVariableBinding> VariableBindings { get; }

        /// <summary>
        /// The CHC head relation
        /// </summary>
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

        /// Body relations in this CHC
        /// </summary>
        public IReadOnlyList<SemanticRelation> BodyRelations { get; }

        /// <summary>
        /// The CHC constraint, as an SMT expression
        /// </summary>
        public SmtTerm Constraint { get; }

        /// <summary>
        /// The CHC operator binder
        /// </summary>
        public SmtMatchBinder Binder { get; }

        /// <summary>
        /// Creates a CHC from the given components
        /// </summary>
        /// <param name="head">The CHC head</param>
        /// <param name="childRels">Child relations in the CHC body</param>
        /// <param name="constraint">The CHC constraint as an SMT term</param>
        /// <param name="binder">The match binder for the operator and child terms</param>
        /// <param name="bindings">Variable bindings</param>
        /// <param name="inputs">Input variables</param>
        /// <param name="outputs">Output variables</param>
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

        /// <summary>
        /// A semantic relation usage, consisting of a relation, its rank, and the actuals passed to it
        /// </summary>
        /// <param name="Relation">The relation</param>
        /// <param name="Rank">The specific relation rank</param>
        /// <param name="Arguments">The actuals passed to the relation</param>
        public record SemanticRelation(IApplicable Relation, SmtFunctionRank Rank, IReadOnlyList<SmtVariable> Arguments)
        {
            /// <summary>
            /// Gets this relation as a string
            /// </summary>
            /// <returns>The relation as a string</returns>
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
            /// <summary>
            /// Symbols known to be inputs
            /// </summary>
            public IReadOnlyCollection<SymbolEntry> Inputs { get; }

            /// <summary>
            /// Symbols known to be outputs
            /// </summary>
            public IReadOnlyCollection<SymbolEntry> Outputs { get; }

            /// <summary>
            /// The term symbol
            /// </summary>
            public SymbolEntry Term { get; }

            /// <summary>
            /// Symbols that we don't know if are inputs, outputs, or the term
            /// </summary>
            public IReadOnlyCollection<SymbolEntry> Unclassified { get; }

            /// <summary>
            /// Symbols bound only in the CHC body
            /// </summary>
            public IReadOnlyCollection<SymbolEntry> Auxiliary { get; }

            /// <summary>
            /// Symbols for child terms
            /// </summary>
            public IReadOnlyCollection<SymbolEntry> Children { get; }

            /// <summary>
            /// The CHC associated with this table
            /// </summary>
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
                else
                {
                    Inputs = new List<SymbolEntry>();
                }

                if (chc.OutputVariables is not null)
                {
                    Outputs = chc.OutputVariables.Select(ConvertVariable).ToList();
                }
                else
                {
                    Outputs = new List<SymbolEntry>();
                }

                Term = ConvertVariable(_chc.TermVariable);
                Auxiliary = chc.AuxiliaryVariables.Select(ConvertBinding).ToList();
                Children = chc.Binder.Bindings.Select(ConvertMatchBinding).ToList();

                List<SymbolEntry> unclassified = new();
                foreach (var arg in chc.Head.Arguments)
                {
                    if (arg.Name != Term.Id && !ContainsVar(Inputs, arg) && !ContainsVar(Outputs, arg))
                    {
                        unclassified.Add(ConvertVariable(arg));
                    }
                }
                Unclassified = unclassified;

                // Checks if a collection of symbol entries has an entry for the given variable
                // We do this manually to avoid pre-emptively converting between SymbolEntry and SmtVariable.
                static bool ContainsVar(IReadOnlyCollection<SymbolEntry> coll, SmtVariable var)
                {
                    foreach (var se in coll)
                    {
                        if (se.Id == var.Name)
                        {
                            return true;
                        }
                    }
                    return false;
                }
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
