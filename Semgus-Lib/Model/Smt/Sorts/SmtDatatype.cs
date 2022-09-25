using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model.Smt.Sorts
{
    /// <summary>
    /// Algebraic datatype in SMT
    /// </summary>
    public class SmtDatatype : SmtSort
    {
        /// <summary>
        /// Creates a new datatype with the given name
        /// </summary>
        /// <param name="name">The datatype name</param>
        public SmtDatatype(SmtSortIdentifier name) : base(name)
        {
        }

        /// <summary>
        /// Adds a constructor to this datatype
        /// </summary>
        /// <param name="constructor">The constructor to add</param>
        /// <exception cref="InvalidOperationException">Thrown if this datatype is frozen</exception>
        public void AddConstructor(SmtDatatypeConstructor constructor)
        {
            if (_frozen)
            {
                throw new InvalidOperationException("Cannot add constructor to frozen datatype.");
            }
            _constructors.Add(constructor);
        }

        /// <summary>
        /// Freezes this datatype, so no new constructors can be added
        /// </summary>
        public void Freeze()
        {
            _frozen = true;
        }

        /// <summary>
        /// Flag if new constructors are allowed to be added
        /// </summary>
        private bool _frozen = false;

        /// <summary>
        /// Underlying storage for constructors
        /// </summary>
        private readonly List<SmtDatatypeConstructor> _constructors = new();

        /// <summary>
        /// Collection of all constructors associated with this datatype
        /// </summary>
        public IReadOnlyCollection<SmtDatatypeConstructor> Constructors => _constructors;
    }
}
