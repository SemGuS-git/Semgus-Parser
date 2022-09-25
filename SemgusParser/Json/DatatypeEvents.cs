using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Sorts;

namespace Semgus.Parser.Json
{
    /// <summary>
    /// Event for forward-declaring datatypes
    /// </summary>
    internal class DatatypeDeclarationEvent : ParseEvent
    {
        /// <summary>
        /// The datatype name
        /// </summary>
        public SmtSortIdentifier Name { get; }

        /// <summary>
        /// The datatype arity (how many sort parameters it has)
        /// </summary>
        public int Arity { get; }

        /// <summary>
        /// Constructs a new DatatypeDeclarationEvent
        /// </summary>
        /// <param name="dt">The datatype to forward-declare</param>
        public DatatypeDeclarationEvent(SmtDatatype dt) : base("declare-datatype", "smt")
        {
            Name = dt.Name;
        }
    }

    /// <summary>
    /// Event for associating constructors with a datatype
    /// </summary>
    internal class DatatypeDefinitionEvent : ParseEvent
    {
        /// <summary>
        /// Datatype name that constructors are associated with
        /// </summary>
        public SmtSortIdentifier Name { get; }

        /// <summary>
        /// Constructors for the given datatype
        /// </summary>
        public IEnumerable<ConstructorModel> Constructors { get; }

        /// <summary>
        /// Constructs a new datatype definition event
        /// </summary>
        /// <param name="dt">Datatype to define</param>
        public DatatypeDefinitionEvent(SmtDatatype dt) : base("define-datatype", "smt")
        {
            Name = dt.Name;
            Constructors = dt.Constructors.Select(c => new ConstructorModel(c.Name, c.Children.Select(x => x.Name)));
        }

        /// <summary>
        /// Record for holding name/children pairs for constructors.
        /// Note that selectors are _not_ included, as they are "banned" from SemGuS
        /// </summary>
        /// <param name="Name">The constructor name</param>
        /// <param name="Children">The constructor children</param>
        public record ConstructorModel(SmtIdentifier Name, IEnumerable<SmtSortIdentifier> Children);
    }
}
