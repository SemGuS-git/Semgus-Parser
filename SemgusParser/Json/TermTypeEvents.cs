using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model;
using Semgus.Model.Smt;

namespace Semgus.Parser.Json
{
    /// <summary>
    /// Event for forward-declaring term types
    /// </summary>
    internal class TermTypeDeclarationEvent : ParseEvent
    {
        /// <summary>
        /// The term type name
        /// </summary>
        public SmtSortIdentifier Name { get; }

        /// <summary>
        /// Constructs a new term type declaration event
        /// </summary>
        /// <param name="tt">Term type to forward-declare</param>
        public TermTypeDeclarationEvent(SemgusTermType tt) : base("declare-term-type", "semgus")
        {
            Name = tt.Name;
        }
    }

    /// <summary>
    /// Event for defining constructors associated with a term type
    /// </summary>
    internal class TermTypeDefinitionEvent : ParseEvent
    {
        /// <summary>
        /// Name of term type to associate constructors to
        /// </summary>
        public SmtSortIdentifier Name { get; }

        /// <summary>
        /// List of term constructors
        /// </summary>
        public IEnumerable<ConstructorModel> Constructors { get; }

        /// <summary>
        /// Constructs a new term type definition event
        /// </summary>
        /// <param name="tt">Term type to define</param>
        public TermTypeDefinitionEvent(SemgusTermType tt) : base("define-term-type", "semgus")
        {
            Name = tt.Name;
            Constructors = tt.Constructors.Select(c => new ConstructorModel(c.Operator, c.Children.Select(x => x.Name)));
        }

        /// <summary>
        /// Record for holding name/children pairs for constructors
        /// </summary>
        /// <param name="Name">The constructor name</param>
        /// <param name="Children">The constructor children</param>
        public record ConstructorModel(SmtIdentifier Name, IEnumerable<SmtSortIdentifier> Children);
    }
}
